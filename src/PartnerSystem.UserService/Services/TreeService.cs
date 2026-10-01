using Microsoft.EntityFrameworkCore;
using PartnerSystem.Contracts.Dtos.Users;
using PartnerSystem.UserService.Data;
using PartnerSystem.UserService.Data.Entities;
using PartnerSystem.UserService.Services.Interfaces;

namespace PartnerSystem.UserService.Services;

public sealed  class TreeService(
    UserDbContext db
) : ITreeService
{
    private const int MaxDepth = 10;

    public async Task<List<long>> GetPartnerChainUpAsync(long userExternalId, CancellationToken cancellationToken)
    {
        var chain = new List<long>();
        
        var current = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.ExternalId == userExternalId, cancellationToken);

        var visited = new HashSet<long> { userExternalId };

        while (current?.PartnerExternalId is {} partnerId && chain.Count < MaxDepth)
        {
            if (!visited.Add(partnerId))
            {
                throw new InvalidOperationException("Cycle detected in the partner tree.");
            }

            chain.Add(partnerId);
            
            current = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.ExternalId == partnerId, cancellationToken);
        }

        return chain;
    }

    public async Task<TreeNodeDto?> GetTreeDownAsync(
        long userExternalId, CancellationToken cancellationToken)
    {
        var root = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.ExternalId == userExternalId, cancellationToken);

        if (root is null) return null;

        // Load all descendants in one query, then build the tree in memory.
        var descendants = await GetDownstreamFlatAsync(userExternalId, cancellationToken);

        var byParent = descendants
            .GroupBy(u => u.PartnerExternalId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        return BuildNode(root, byParent, depth: 0);

        TreeNodeDto BuildNode(UserEntity user, Dictionary<long, List<UserEntity>> childrenByParent, int depth)
        {
            if (depth >= MaxDepth || !childrenByParent.TryGetValue(user.ExternalId, out var children))
                return new TreeNodeDto(user.ExternalId, user.Name, []);

            var childNodes = children
                .Select(c => BuildNode(c, childrenByParent, depth + 1))
                .ToList();

            return new TreeNodeDto(user.ExternalId, user.Name, childNodes);
        }
    }
    
    private async Task<List<UserEntity>> GetDownstreamFlatAsync(long userExternalId, CancellationToken cancellationToken)
    {
        var result = new List<UserEntity>();
        var queue = new Queue<(long id, int depth)>();
        
        queue.Enqueue((userExternalId, 0));

        while (queue.Count > 0)
        {
            var (id, depth) = queue.Dequeue();
            
            if (depth >= MaxDepth)
            {
                continue;
            }

            var children = await db.Users
                .AsNoTracking()
                .Where(u => u.PartnerExternalId == id)
                .ToListAsync(cancellationToken);

            foreach (var child in children)
            {
                result.Add(child);
                queue.Enqueue((child.ExternalId, depth + 1));
            }
        }

        return result;
    }

    public async Task<bool> WouldCreateCycleAsync(long userExternalId, long partnerExternalId, CancellationToken cancellationToken)
    {
        var current = partnerExternalId;
        var visited = new HashSet<long>();

        while (true)
        {
            if (!visited.Add(current))
            {
                return true;
            }

            if (current == userExternalId)
            {
                return true;
            }

            var user = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.ExternalId == current, cancellationToken);

            if (user?.PartnerExternalId is not {} next)
            {
                return false;
            }
            
            current = next;
        }
    }
}