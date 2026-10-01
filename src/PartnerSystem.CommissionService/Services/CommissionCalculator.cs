using PartnerSystem.Shared.Contracts.Enums;

namespace PartnerSystem.CommissionService.Services;

public static class CommissionCalculator
{
    public static decimal Calculate(decimal profit, int level, CommissionSchema schema)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(level, 1);

        if (profit <= 0)
        {
            return 0m;
        }

        decimal multiplier = schema switch
        {
            CommissionSchema.Linear => level,
            CommissionSchema.Fibonacci => Fibonacci(level),
            _ => throw new ArgumentOutOfRangeException(nameof(schema))
        };

        return multiplier * profit / 100m;
    }

    /// <summary>
    /// Calculates partner commissions for a single event across the partner chain.
    /// </summary>
    /// <remarks>
    /// The iterative <see cref="Fibonacci"/> implementation is optimal under the assumption
    /// that <c>n ≤ 10</c> (the maximum partner tree depth enforced by <c>TreeService.MaxDepth</c>).
    /// Within this range it runs in O(n) time and O(1) space, without recursion or allocations.
    /// </remarks>
    public static int Fibonacci(int n)
    {
        switch (n)
        {
            case <= 0:
                return 0;
            case 1 or 2:
                return 1;
        }

        int a = 1, b = 1;
        
        for (var i = 3; i <= n; i++)
        {
            var c = a + b;
            
            a = b;
            b = c;
        }
        
        return b;
    }
}
