using SolarSystem.Application.Common.Interfaces;

namespace SolarSystem.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly SolarDbContext _context;

    public UnitOfWork(SolarDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct = default)
    {
        // Transacao aninhada nao e suportada: se ja existe uma em andamento, participa dela.
        if (_context.Database.CurrentTransaction is not null)
        {
            await action(ct);
            return;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            await action(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}
