# PartnerSystem

Распределённая система партнёрских отчислений: набор микросервисов на **.NET 9**, взаимодействующих через **Kafka** и **HTTP (Refit)**, с поддержкой двух схем начисления, идемпотентностью и отказоустойчивостью.

---

## Быстрый старт

```bash
docker compose up --build -d
```

- Миграции EF Core применяются автоматически при старте сервисов.
- Seed-данные (пользователи, кошельки, события, комиссии) создаются при первом запуске.
- Полный сброс БД: `docker compose down -v`.

---

## Сервисы и порты

| Сервис | Порт | Swagger | Назначение |
|---|---|---|---|
| UserService | 8081 | http://localhost:8081/swagger | Пользователи, партнёрские связи, дерево |
| EventService | 8082 | http://localhost:8082/swagger | Приём событий о прибыли, Outbox → Kafka |
| CommissionService | 8083 | http://localhost:8083/swagger | Расчёт комиссий, история, схема начисления |
| WalletService | 8084 | http://localhost:8084/swagger | Кошельки, балансы, периодические выплаты |
| AdminService | 8085 | http://localhost:8085/swagger | Администрирование |
| Kafdrop (UI для Kafka) | 9000 | http://localhost:9000 | Просмотр топиков и сообщений |
| Kafka | 9092 | — | Брокер (KRaft) |
| PostgreSQL | 5432–5435 | — | 4 отдельные БД |

---

## Архитектура

```
                     ┌───────────────────────┐
                     │   AdminService :8085  │
                     └───────────┬───────────┘
                                 │ Refit (HTTP)
                 ┌───────────────┼────────────────┐
                 ▼                                ▼
     ┌──────────────────────┐        ┌──────────────────────┐
     │ CommissionService    │        │  EventService :8082  │
     │ :8083                │        │                      │
     └────┬─────────────┬───┘        └──────────┬───────────┘
          │ Refit       │ Kafka                 │ Outbox
          ▼             ▲                       ▼
     ┌─────────┐   ┌────┴────┐           ┌───────────┐
     │UserSvc  │   │ Kafka   │           │ Event DB  │
     │:8081    │   │ :9092   │           └───────────┘
     └─────────┘   └────┬────┘
                        │ Kafka
                        ▼
                ┌──────────────────────┐
                │ WalletService :8084  │
                └──────────┬───────────┘
                           │ Refit (HTTP)
                           ▼
                   CommissionService
```

**Поток данных:**

1. `EventService` принимает событие → пишет `EventEntity` + `OutboxMessage` в одной транзакции.
2. `OutboxPublisher` читает Outbox и публикует сообщение в Kafka-топик `profit-events`.
3. `KafkaConsumer` в `CommissionService` читает событие, получает цепочку партнёров через `IUsersApi` (Refit), рассчитывает комиссии, сохраняет их.
4. `PayoutBackgroundService` в `WalletService` каждые 30 секунд собирает невыплаченные комиссии через `ICommissionsApi` (Refit) и выплачивает их на кошелёк.
5. `AdminService` проксирует чтение и администрирование.

---

## Про решение с одним солюшеном

По заданию требуется **микросервисная архитектура**. Формально это означает **отдельные решения** (`.sln`) для каждого сервиса, независимую сборку, независимый деплой, отдельные NuGet-пакеты для контрактов.

Однако в рамках тестового задания **осознанно принято решение разместить все сервисы в одном решении `PartnerSystem.sln`** — для экономии времени, упрощения навигации и единообразной сборки. При этом **границы между сервисами соблюдены**:

- каждый сервис — отдельный проект (`PartnerSystem.UserService`, `PartnerSystem.CommissionService`, ...);
- каждый сервис имеет свою БД и своё подключение;
- сервисы не ссылаются друг на друга **напрямую** — только через `PartnerSystem.Contracts` (DTO + Refit-интерфейсы);
- каждый сервис собирается в отдельный Docker-образ и запускается как отдельный контейнер;

### Взаимодействие через Refit

Все HTTP-вызовы между сервисами типизированы через **Refit**. Интерфейсы (`IUsersApi`, `ICommissionsApi`, `IEventsApi`) и DTO лежат в общем проекте `PartnerSystem.Contracts`, который можно упаковать в NuGet.

Это даёт:
- **интеграцию с resilience** — retry/circuit breaker/timeout навешиваются одной строкой;
- **устойчивость к ошибкам** — `RefitExceptionHandler` читает `ApiError` из тела и транслирует в `DownstreamException` (502), не теряя сообщение.

Регистрация клиента:

```csharp
services.AddPartnerRefitClient<ICommissionsApi>(
    configuration, "Services:CommissionService:BaseUrl");
```

Внутри — `AddRefitClient<T>()` + `AddStandardResilienceHandler()` + `RefitExceptionHandler`.

---

## Ключевые архитектурные решения

### Kafka + Transactional Outbox

`EventService` пишет событие и Outbox-сообщение **в одной транзакции**, а фоновый `OutboxPublisher` публикует их в Kafka. Это исключает рассинхронизацию «в БД есть, в Kafka нет».

Топик `profit-events` создаётся автоматически.

### Идемпотентность на трёх уровнях

| Уровень | Механизм |
|---|---|
| HTTP | `EventService.Create` проверяет `EventId` → повторный POST вернёт `already_exists` |
| Kafka consumer | `AnyAsync(c => c.EventId == message.EventId)` перед вставкой |
| БД | Уникальные индексы: `EventEntity.EventId`, `CommissionEntity (EventId, BeneficiaryUserId)`, `PaymentEntity.CommissionId` |

Гарантия **at-least-once** без дублей.

### Две схемы начисления

| Схема | Формула | L1 | L2 | L3 | L4 |
|---|---|---|---|---|---|
| Linear | `L × Profit / 100` | 10 | 20 | 30 | 40 |
| Fibonacci | `F(L) × Profit / 100` | 10 | 10 | 20 | 30 |

Схема хранится в БД (`CommissionSchemaEntity`, singleton `Id = 1`). **Каждая комиссия хранит `SchemaType`** — переключение влияет только на новые расчёты.

### Отказоустойчивость

- **HTTP (Refit)**: retry (3 попытки, exponential backoff), circuit breaker (failure ratio 50%), timeout (30 с/10 с).
- **Kafka**: offset коммитится только после успешной обработки; poison messages логируются и коммитятся, чтобы не блокировать consumer group.
- **Outbox / Payout**: retry через polling.
- **Graceful shutdown**: `CancellationToken` во всех `BackgroundService`; offset не коммитится при отмене; `KafkaProducer.Flush(5s)` на Dispose.

### Общие контракты

- **`PartnerSystem.Shared`** — `ApiError`, `AppException`, `GlobalExceptionHandler`, `RefitExceptionHandler`, health check tags/paths, enum'ы, `JsonOptions`.
- **`PartnerSystem.Contracts`** — Refit-интерфейсы и DTO.

---

## Тестовые данные (seed)

Создаются автоматически при первом запуске через `HasData` в EF Core Migrations.

### Пользователи

| Имя | ExternalId | Партнёр |
|---|---|---|
| Alice | 1 | — |
| Bob | 2 | Alice |
| Charlie | 3 | Bob |
| Dave | 4 | Charlie |
| Eve | 5 | Dave |
| Frank | 6 | — |

Иерархия:

```
Alice
  └── Bob
        └── Charlie
              └── Dave
                    └── Eve
Frank (отдельная ветка)
```

### События

| EventId | Источник | Profit |
|---|---|---|
| 1000 | Eve | 1000 |
| 1001 | Frank | 500 |

### Комиссии для EventId=1000 (Linear)

| Уровень | Бенефициар | Сумма |
|---|---|---|
| 1 | Dave | 10 |
| 2 | Charlie | 20 |
| 3 | Bob | 30 |
| 4 | Alice | 40 |

Все со статусом `Accrued`. После первого payout-цикла (30 с) → `Paid`, балансы кошельков обновятся.

### Кошельки

Созданы для всех 6 пользователей, начальный баланс — 0.

---

## Тестовые сценарии

### Сценарий 1 — Проверка предзасеянных данных

```bash
# Пользователь
curl http://localhost:8081/api/users/5
# → {"externalId":5,"name":"Eve","partnerExternalId":4}

# Цепочка вверх
curl http://localhost:8081/api/users/5/chain-up
# → [4, 3, 2, 1]

# Комиссии по событию 1000
curl http://localhost:8083/api/commissions/by-event/1000
# → 4 комиссии: Dave 10, Charlie 20, Bob 30, Alice 40 (Accrued)

# Через 30 секунд балансы:
curl http://localhost:8084/api/wallets/1/balance   # Alice = 40
curl http://localhost:8084/api/wallets/2/balance   # Bob   = 30
curl http://localhost:8084/api/wallets/3/balance   # Charlie = 20
curl http://localhost:8084/api/wallets/4/balance   # Dave  = 10
```

### Сценарий 2 — End-to-end: новое событие

```bash
# 1. Отправить событие от Eve
curl -X POST http://localhost:8082/api/events \
  -H "Content-Type: application/json" \
  -d '{"eventId":2000,"userExternalId":5,"profit":1000,"occurredAt":"2026-01-01T00:00:00Z"}'

# 2. Подождать 2–3 с и открыть Kafdrop
# http://localhost:9000 → topic profit-events → увидеть сообщение

# 3. Проверить комиссии
curl http://localhost:8083/api/commissions/by-event/2000
# → 4 комиссии (Linear): 10, 20, 30, 40

# 4. Проверка идемпотентности — отправить то же событие снова
curl -X POST http://localhost:8082/api/events \
  -H "Content-Type: application/json" \
  -d '{"eventId":2000,"userExternalId":5,"profit":1000,"occurredAt":"2026-01-01T00:00:00Z"}'

curl http://localhost:8083/api/commissions/by-event/2000
# → всё ещё 4 комиссии (не 8)
```

### Сценарий 3 — Переключение схемы начисления

```bash
# 1. Текущая схема
curl http://localhost:8083/api/commissions/schema
# → {"schema":"Linear"}

# 2. Переключить на Fibonacci
curl -X PUT http://localhost:8083/api/commissions/schema \
  -H "Content-Type: application/json" \
  -d '{"schema":1}'

# 3. Отправить новое событие
curl -X POST http://localhost:8082/api/events \
  -H "Content-Type: application/json" \
  -d '{"eventId":3000,"userExternalId":5,"profit":1000,"occurredAt":"2026-01-01T00:00:00Z"}'

# 4. Проверить комиссии (Fibonacci)
curl http://localhost:8083/api/commissions/by-event/3000
# → Dave 10, Charlie 10, Bob 20, Alice 30

# 5. Убедиться, что старые комиссии не пересчитались
curl http://localhost:8083/api/commissions/by-event/1000
# → по-прежнему Linear, 10/20/30/40
```

### Сценарий 4 — Kafka UI (Kafdrop)

1. Открыть http://localhost:9000
2. Топик `profit-events` → список сообщений
3. Раздел **Consumers** → группа `commission-service`, посмотреть `LAG` (0 при нормальной работе)

---

## Полезные команды

```bash
# Логи
docker compose logs -f commission-service

# Перезапуск одного сервиса
docker compose up --build -d commission-service

# Список топиков
docker exec -it kafka /opt/kafka/bin/kafka-topics.sh \
  --bootstrap-server localhost:9092 --list

# Последние сообщения
docker exec -it kafka /opt/kafka/bin/kafka-console-consumer.sh \
  --bootstrap-server localhost:9092 \
  --topic profit-events --from-beginning --max-messages 10

# Consumer group lag
docker exec -it kafka /opt/kafka/bin/kafka-consumer-groups.sh \
  --bootstrap-server localhost:9092 \
  --describe --group commission-service
```

---

## Структура репозитория

```
PartnerSystem/
├── docker-compose.yml
├── PartnerSystem.sln
├── README.md
├── src/
│   ├── PartnerSystem.Shared/         ← ApiError, exceptions, Refit/health-инфраструктура
│   ├── PartnerSystem.Contracts/      ← Refit-интерфейсы + DTO
│   ├── PartnerSystem.UserService/
│   ├── PartnerSystem.EventService/
│   ├── PartnerSystem.CommissionService/
│   ├── PartnerSystem.WalletService/
│   └── PartnerSystem.AdminService/
└── tests/
    └── PartnerSystem.Tests/          ← unit-тесты CommissionCalculator
```

---

## Тесты

```bash
dotnet test
```

Покрыто: `CommissionCalculator` (Linear, Fibonacci, граничные случаи, отрицательный Profit).

## Что можно было бы улучшить

Было принято решение осознанно не тянуть в тестовое задание то, что было бы оверинжинирингом и увеличило бы время разработки без пропорциональной пользы:

- **Отдельные .sln и NuGet-пакеты** для каждого сервиса и контрактов. Сейчас всё в одном решении — для экономии времени; границы сервисов соблюдены (свои БД, Dockerfile, общение только через `PartnerSystem.Contracts`).
- **Миграции через отдельный job**, а не `Migrate()` при старте.
- **DLQ для Kafka** вместо «залогировал и закоммитил» для poison messages.
- **Метрики (Prometheus/OpenTelemetry) и распределённый трейсинг** через Kafka/HTTP.
- **Распределённый лок** для payout, чтобы реплики `WalletService` не конкурировали.
- **Интеграционные тесты с Testcontainers** (PostgreSQL + Kafka end-to-end), contract-тесты между сервисами.
- **Saga/Process Manager** для явной компенсации распределённых операций.
- **Оптимизация `TreeService`** на рекурсивный SQL вместо N+1 обхода дерева.

Каждый пункт — отдельный день работы и отдельный слой инфраструктуры. Для тестового задания важнее было показать корректную работу основной логики (начисление, идемпотентность, две схемы, Outbox + Kafka).