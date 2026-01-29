namespace FluxoCaixa.API.Controllers;

using Application.DTOs;
using Application.UseCases;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ConsolidadoController : ControllerBase
{
    private readonly ObterConsolidadoDiarioUseCase _obterConsolidadoUseCase;
    private readonly ILogger<ConsolidadoController> _logger;

    public ConsolidadoController(
        ObterConsolidadoDiarioUseCase obterConsolidadoUseCase,
        ILogger<ConsolidadoController> logger)
    {
        _obterConsolidadoUseCase = obterConsolidadoUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Obtém o consolidado diário de uma data específica
    /// </summary>
    [HttpGet("{data}")]
    [ProducesResponseType(typeof(ConsolidadoDiarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConsolidadoDiarioDto>> ObterPorData(string data)
    {
        if (!DateTime.TryParse(data, out var dataParsed))
        {
            return BadRequest(new { message = "Data inválida. Use o formato yyyy-MM-dd" });
        }

        var resultado = await _obterConsolidadoUseCase.ExecutarAsync(dataParsed);

        if (resultado == null)
        {
            return NotFound(new { message = "Consolidado não encontrado para esta data" });
        }

        return Ok(resultado);
    }

    /// <summary>
    /// Obtém consolidados de um período
    /// </summary>
    [HttpGet("range")]
    [ProducesResponseType(typeof(IEnumerable<ConsolidadoDiarioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ConsolidadoDiarioDto>>> ObterPorPeriodo(
        [FromQuery] string dataInicio,
        [FromQuery] string dataFim)
    {
        if (!DateTime.TryParse(dataInicio, out var dataInicioParsed) ||
            !DateTime.TryParse(dataFim, out var dataFimParsed))
        {
            return BadRequest(new { message = "Datas inválidas. Use o formato yyyy-MM-dd" });
        }

        if (dataInicioParsed > dataFimParsed)
        {
            return BadRequest(new { message = "Data inicial não pode ser maior que data final" });
        }

        var resultado = await _obterConsolidadoUseCase.ExecutarPorPeriodoAsync(dataInicioParsed, dataFimParsed);
        return Ok(resultado);
    }
}
