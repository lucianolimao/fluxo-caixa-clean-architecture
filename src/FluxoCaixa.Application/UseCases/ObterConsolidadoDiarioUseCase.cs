namespace FluxoCaixa.Application.UseCases;

using Domain.Entities;
using Domain.Interfaces;
using DTOs;

public class ObterConsolidadoDiarioUseCase
{
    private readonly IConsolidadoDiarioRepository _consolidadoRepository;

    public ObterConsolidadoDiarioUseCase(IConsolidadoDiarioRepository consolidadoRepository)
    {
        _consolidadoRepository = consolidadoRepository;
    }

    public async Task<ConsolidadoDiarioDto?> ExecutarAsync(DateTime data)
    {
        var consolidado = await _consolidadoRepository.ObterPorDataAsync(data);
        return consolidado != null ? MapearParaDto(consolidado) : null;
    }

    public async Task<IEnumerable<ConsolidadoDiarioDto>> ExecutarPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        var consolidados = await _consolidadoRepository.ObterPorPeriodoAsync(dataInicio, dataFim);
        return consolidados.Select(MapearParaDto);
    }

    private static ConsolidadoDiarioDto MapearParaDto(ConsolidadoDiario consolidado)
    {
        return new ConsolidadoDiarioDto
        {
            Data = consolidado.Data,
            TotalCreditos = consolidado.TotalCreditos,
            TotalDebitos = consolidado.TotalDebitos,
            SaldoFinal = consolidado.SaldoFinal,
            QuantidadeLancamentos = consolidado.QuantidadeLancamentos,
            UltimaAtualizacao = consolidado.UltimaAtualizacao
        };
    }
}
