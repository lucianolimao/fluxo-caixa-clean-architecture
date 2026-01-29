namespace FluxoCaixa.Infrastructure.Data;

using Domain.Interfaces;

public class UnitOfWork(FluxoCaixaDbContext context) : IUnitOfWork
{
    public async Task<int> CommitAsync()
    {
        return await context.SaveChangesAsync();
    }

    public Task RollbackAsync()
    {
        return Task.CompletedTask;
    }
}
