using Microsoft.EntityFrameworkCore;
using PartnerSystem.UserService.Data;
using PartnerSystem.UserService.Data.Entities;
using PartnerSystem.UserService.Services;

public sealed class TreeServiceTests
{
    // Must match TreeService.MaxDepth. If MaxDepth becomes configurable via TreeOptions,
    // inject it into the service in the helper methods below.
    private const int MaxDepth = 10;

    private static UserDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new UserDbContext(options);
    }

    private static async Task<UserDbContext> CreateContextWithUsersAsync(
        params (long ExternalId, string Name, long? PartnerExternalId)[] users)
    {
        var db = CreateContext();

        foreach (var (externalId, name, partnerExternalId) in users)
        {
            db.Users.Add(new UserEntity
            {
                Id = externalId,
                ExternalId = externalId,
                Name = name,
                PartnerExternalId = partnerExternalId
            });
        }

        await db.SaveChangesAsync();
        return db;
    }

    private static Task<UserDbContext> CreateStandardChainAsync() =>
        CreateContextWithUsersAsync(
            (1, "Alice",   null),
            (2, "Bob",     1),
            (3, "Charlie", 2),
            (4, "Dave",    3),
            (5, "Eve",     4));

    [Fact]
    public async Task GetPartnerChainUpAsync_ReturnsEmpty_WhenUserHasNoPartner()
    {
        await using var db = await CreateStandardChainAsync();
        var service = new TreeService(db);

        var chain = await service.GetPartnerChainUpAsync(1, default);

        Assert.Empty(chain);
    }

    [Fact]
    public async Task GetPartnerChainUpAsync_ReturnsEmpty_WhenUserDoesNotExist()
    {
        await using var db = await CreateStandardChainAsync();
        var service = new TreeService(db);

        var chain = await service.GetPartnerChainUpAsync(999, default);

        Assert.Empty(chain);
    }

    [Fact]
    public async Task GetPartnerChainUpAsync_ReturnsDirectPartner_ForFirstLevel()
    {
        await using var db = await CreateStandardChainAsync();
        var service = new TreeService(db);

        var chain = await service.GetPartnerChainUpAsync(2, default);

        Assert.Equal(new[] { 1L }, chain);
    }

    [Fact]
    public async Task GetPartnerChainUpAsync_ReturnsFullChain_ForDeepUser()
    {
        await using var db = await CreateStandardChainAsync();
        var service = new TreeService(db);

        var chain = await service.GetPartnerChainUpAsync(5, default);

        // Eve → Dave(4) → Charlie(3) → Bob(2) → Alice(1)
        Assert.Equal(new[] { 4L, 3L, 2L, 1L }, chain);
    }

    [Fact]
    public async Task GetPartnerChainUpAsync_StopsAtMaxDepth()
    {
        // Chain of 15 users: 1 ← 2 ← 3 ← ... ← 15
        var users = new List<(long, string, long?)> { (1, "User1", null) };
        for (var i = 2; i <= 15; i++)
            users.Add((i, $"User{i}", i - 1));

        await using var db = await CreateContextWithUsersAsync(users.ToArray());
        var service = new TreeService(db);

        var chain = await service.GetPartnerChainUpAsync(15, default);

        Assert.Equal(MaxDepth, chain.Count);
    }

    [Fact]
    public async Task GetPartnerChainUpAsync_Throws_WhenCycleDetected()
    {
        // A(1) → C(3), B(2) → A(1), C(3) → B(2) forms A → C → B → A
        await using var db = await CreateContextWithUsersAsync(
            (1, "A", 3),
            (2, "B", 1),
            (3, "C", 2));

        var service = new TreeService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetPartnerChainUpAsync(2, default));
    }

    [Fact]
    public async Task GetTreeDownAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        await using var db = await CreateStandardChainAsync();
        var service = new TreeService(db);

        var tree = await service.GetTreeDownAsync(999, default);

        Assert.Null(tree);
    }

    [Fact]
    public async Task GetTreeDownAsync_ReturnsRootWithoutChildren_ForLeaf()
    {
        await using var db = await CreateStandardChainAsync();
        var service = new TreeService(db);

        var tree = await service.GetTreeDownAsync(5, default); // Eve

        Assert.NotNull(tree);
        Assert.Equal(5, tree!.ExternalId);
        Assert.Empty(tree.Children);
    }

    [Fact]
    public async Task GetTreeDownAsync_ReturnsDirectChild()
    {
        await using var db = await CreateStandardChainAsync();
        var service = new TreeService(db);

        var tree = await service.GetTreeDownAsync(4, default); // Dave

        Assert.NotNull(tree);
        Assert.Equal(4, tree!.ExternalId);
        Assert.Single(tree.Children);
        Assert.Equal(5, tree.Children[0].ExternalId);
    }

    [Fact]
    public async Task GetTreeDownAsync_ReturnsNestedTree()
    {
        await using var db = await CreateStandardChainAsync();
        var service = new TreeService(db);

        var tree = await service.GetTreeDownAsync(2, default); // Bob

        Assert.NotNull(tree);
        Assert.Equal(2, tree!.ExternalId);

        var charlie = Assert.Single(tree.Children);
        Assert.Equal(3, charlie.ExternalId);

        var dave = Assert.Single(charlie.Children);
        Assert.Equal(4, dave.ExternalId);

        var eve = Assert.Single(dave.Children);
        Assert.Equal(5, eve.ExternalId);

        Assert.Empty(eve.Children);
    }

    [Fact]
    public async Task GetTreeDownAsync_HandlesMultipleBranches()
    {
        await using var db = await CreateContextWithUsersAsync(
            (1, "Root",    null),
            (2, "Branch1", 1),
            (3, "Branch2", 1),
            (4, "Leaf1",   2),
            (5, "Leaf2",   3));

        var service = new TreeService(db);

        var tree = await service.GetTreeDownAsync(1, default);

        Assert.NotNull(tree);
        Assert.Equal(2, tree!.Children.Count);

        var branch1 = Assert.Single(tree.Children.Where(c => c.ExternalId == 2));
        var branch2 = Assert.Single(tree.Children.Where(c => c.ExternalId == 3));

        Assert.Equal(4, Assert.Single(branch1.Children).ExternalId);
        Assert.Equal(5, Assert.Single(branch2.Children).ExternalId);
    }

    [Fact]
    public async Task GetTreeDownAsync_StopsAtMaxDepth()
    {
        // Chain of 15 users: 1 ← 2 ← ... ← 15
        var users = new List<(long, string, long?)> { (1, "User1", null) };
        for (var i = 2; i <= 15; i++)
            users.Add((i, $"User{i}", i - 1));

        await using var db = await CreateContextWithUsersAsync(users.ToArray());
        var service = new TreeService(db);

        var tree = await service.GetTreeDownAsync(1, default);

        // Walk down the single-child chain and count depth.
        var depth = 0;
        var current = tree;
        while (current is not null && current.Children.Count > 0)
        {
            depth++;
            current = current.Children[0];
        }

        Assert.Equal(MaxDepth, depth);
    }

    [Fact]
    public async Task WouldCreateCycleAsync_ReturnsFalse_ForCleanLink()
    {
        await using var db = await CreateStandardChainAsync();
        var service = new TreeService(db);

        var result = await service.WouldCreateCycleAsync(99, 1, default);

        Assert.False(result);
    }

    [Fact]
    public async Task WouldCreateCycleAsync_ReturnsTrue_ForSelfReference()
    {
        await using var db = await CreateStandardChainAsync();
        var service = new TreeService(db);

        var result = await service.WouldCreateCycleAsync(2, 2, default);

        Assert.True(result);
    }

    [Fact]
    public async Task WouldCreateCycleAsync_ReturnsTrue_WhenLinkClosesExistingChain()
    {
        await using var db = await CreateStandardChainAsync();
        var service = new TreeService(db);

        // Alice(1) → Eve(5): Eve's chain up already contains Alice → cycle.
        var result = await service.WouldCreateCycleAsync(1, 5, default);

        Assert.True(result);
    }

    [Fact]
    public async Task WouldCreateCycleAsync_ReturnsFalse_ForIndependentBranch()
    {
        await using var db = await CreateContextWithUsersAsync(
            (1, "Alice", null),
            (2, "Bob",   1),
            (3, "Frank", null));

        var service = new TreeService(db);

        var result = await service.WouldCreateCycleAsync(3, 2, default);

        Assert.False(result);
    }
}