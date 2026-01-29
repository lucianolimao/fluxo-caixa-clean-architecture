namespace FluxoCaixa.Infrastructure.Repositories;

using Data;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public class ConsolidadoDiarioRepository(FluxoCaixaDbContext context) : IConsolidadoDiarioRepository
{
    public async Task<ConsolidadoDiario?> ObterPorDataAsync(DateTime data)
    {
        return await context.ConsolidadosDiarios
            .FirstOrDefaultAsync(c => c.Data == data);
    }

    public async Task<IEnumerable<ConsolidadoDiario>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        return await context.ConsolidadosDiarios
            .Where(c => c.Data >= dataInicio && c.Data <= dataFim)
            .OrderBy(c => c.Data)
            .ToListAsync();
    }

    public async Task AdicionarAsync(ConsolidadoDiario consolidado)
    {
        await context.ConsolidadosDiarios.AddAsync(consolidado);
    }

    public Task AtualizarAsync(ConsolidadoDiario consolidado)
    {
        context.ConsolidadosDiarios.Update(consolidado);
        return Task.CompletedTask;
    }
}
