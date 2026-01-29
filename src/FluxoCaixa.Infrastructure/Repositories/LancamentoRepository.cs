namespace FluxoCaixa.Infrastructure.Repositories;

using Data;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public class LancamentoRepository(FluxoCaixaDbContext context) : ILancamentoRepository
{
    public async Task<Lancamento?> ObterPorIdAsync(Guid id)
    {
        return await context.Lancamentos
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<Lancamento>> ObterTodosAsync(int pagina, int tamanhoPagina)
    {
        return await context.Lancamentos
            .OrderByDescending(l => l.Data)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();
    }

    public async Task<IEnumerable<Lancamento>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        return await context.Lancamentos
            .Where(l => l.Data >= dataInicio && l.Data <= dataFim)
            .OrderBy(l => l.Data)
            .ToListAsync();
    }

    public async Task<IEnumerable<Lancamento>> ObterPorDataAsync(DateTime data)
    {
        var dataInicio = data.Date;
        var dataFim = data.Date.AddDays(1).AddTicks(-1);

        return await context.Lancamentos
            .Where(l => l.Data >= dataInicio && l.Data <= dataFim)
            .ToListAsync();
    }

    public async Task AdicionarAsync(Lancamento lancamento)
    {
        await context.Lancamentos.AddAsync(lancamento);
    }

    public async Task<int> ContarAsync()
    {
        return await context.Lancamentos.CountAsync();
    }
}
