using PartnerSystem.CommissionService.Services;
using PartnerSystem.Shared.Contracts.Enums;

namespace PartnerSystem.Tests.CommissionServiceTests;

public class CommissionCalculatorTests
{
    // ---------- Линейная схема: L × Profit / 100 ----------

    [Theory]
    [InlineData(100, 1, 1.0)]
    [InlineData(100, 2, 2.0)]
    [InlineData(100, 3, 3.0)]
    [InlineData(100, 10, 10.0)]
    [InlineData(1000, 1, 10.0)]
    [InlineData(1000, 5, 50.0)]
    [InlineData(250, 4, 10.0)]
    public void Linear_ShouldCalculateLevelTimesProfitDividedBy100(decimal profit, int level, decimal expected)
    {
        var result = CommissionCalculator.Calculate(profit, level, CommissionSchema.Linear);
        Assert.Equal(expected, result);
    }

    // ---------- Схема Фибоначчи: F(L) × Profit / 100 ----------
    // F(1)=1, F(2)=1, F(3)=2, F(4)=3, F(5)=5, F(6)=8, F(7)=13, F(8)=21, F(9)=34, F(10)=55

    [Theory]
    [InlineData(100, 1, 1.0)]   // 1 * 100 / 100
    [InlineData(100, 2, 1.0)]   // 1 * 100 / 100
    [InlineData(100, 3, 2.0)]   // 2 * 100 / 100
    [InlineData(100, 4, 3.0)]   // 3 * 100 / 100
    [InlineData(100, 5, 5.0)]   // 5 * 100 / 100
    [InlineData(100, 6, 8.0)]   // 8 * 100 / 100
    [InlineData(1000, 7, 130.0)] // 13 * 1000 / 100
    [InlineData(1000, 10, 550.0)] // 55 * 1000 / 100
    public void Fibonacci_ShouldCalculateFibLevelTimesProfitDividedBy100(decimal profit, int level, decimal expected)
    {
        var result = CommissionCalculator.Calculate(profit, level, CommissionSchema.Fibonacci);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Fibonacci_ValuesAreCorrectForFirst10Levels()
    {
        int[] expected = { 1, 1, 2, 3, 5, 8, 13, 21, 34, 55 };
        for (var i = 1; i <= 10; i++)
        {
            Assert.Equal(expected[i - 1], CommissionCalculator.Fibonacci(i));
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    [InlineData(-1)]
    public void NonPositiveProfit_ShouldReturnZero(decimal profit)
    {
        var linear = CommissionCalculator.Calculate(profit, 3, CommissionSchema.Linear);
        var fib = CommissionCalculator.Calculate(profit, 3, CommissionSchema.Fibonacci);
        Assert.Equal(0m, linear);
        Assert.Equal(0m, fib);
    }

    [Fact]
    public void LevelLessThanOne_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CommissionCalculator.Calculate(100, 0, CommissionSchema.Linear));
    }

    [Fact]
    public void LinearAndFibonacci_DifferFromLevel3()
    {
        // Level 3: Linear = 3, Fibonacci = 2
        var linear = CommissionCalculator.Calculate(100, 3, CommissionSchema.Linear);
        var fib = CommissionCalculator.Calculate(100, 3, CommissionSchema.Fibonacci);
        Assert.NotEqual(linear, fib);
        Assert.Equal(3.0m, linear);
        Assert.Equal(2.0m, fib);
    }

    [Fact]
    public void FractionalProfit_ShouldBeCalculatedCorrectly()
    {
        // 2.5 * 3 / 100 = 0.075
        var result = CommissionCalculator.Calculate(2.5m, 3, CommissionSchema.Linear);
        Assert.Equal(0.075m, result);
    }
}