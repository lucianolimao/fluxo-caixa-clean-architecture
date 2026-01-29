namespace FluxoCaixa.Domain.Interfaces;

using Entities;

public interface ILancamentoRepository
{
    Task<Lancamento?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<Lancamento>> ObterTodosAsync(int pagina, int tamanhoPagina);
    Task<IEnumerable<Lancamento>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
    Task<IEnumerable<Lancamento>> ObterPorDataAsync(DateTime data);
    Task AdicionarAsync(Lancamento lancamento);
    Task<int> ContarAsync();
}
