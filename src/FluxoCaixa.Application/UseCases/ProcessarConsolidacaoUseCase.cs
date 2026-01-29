namespace FluxoCaixa.Application.UseCases;

using Domain.Entities;
using Domain.Interfaces;

public class ProcessarConsolidacaoUseCase
{
    private readonly ILancamentoRepository _lancamentoRepository;
    private readonly IConsolidadoDiarioRepository _consolidadoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessarConsolidacaoUseCase(
        ILancamentoRepository lancamentoRepository,
        IConsolidadoDiarioRepository consolidadoRepository,
        IUnitOfWork unitOfWork)
    {
        _lancamentoRepository = lancamentoRepository;
        _consolidadoRepository = consolidadoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecutarAsync(DateTime data)
    {
        var consolidadoExistente = await _consolidadoRepository.ObterPorDataAsync(data);
        var lancamentosDoDia = await _lancamentoRepository.ObterPorDataAsync(data);

        if (consolidadoExistente != null)
        {
            consolidadoExistente.Recalcular(lancamentosDoDia);
            await _consolidadoRepository.AtualizarAsync(consolidadoExistente);
        }
        else
        {
            var novoConsolidado = new ConsolidadoDiario(data);
            novoConsolidado.Recalcular(lancamentosDoDia);
            await _consolidadoRepository.AdicionarAsync(novoConsolidado);
        }

        await _unitOfWork.CommitAsync();
    }
}
