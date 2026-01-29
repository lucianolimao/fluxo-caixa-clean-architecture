namespace FluxoCaixa.Domain.Interfaces;

using Entities;

public interface IConsolidadoDiarioRepository
{
    Task<ConsolidadoDiario?> ObterPorDataAsync(DateTime data);
    Task<IEnumerable<ConsolidadoDiario>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
    Task AdicionarAsync(ConsolidadoDiario consolidado);
    Task AtualizarAsync(ConsolidadoDiario consolidado);
}
