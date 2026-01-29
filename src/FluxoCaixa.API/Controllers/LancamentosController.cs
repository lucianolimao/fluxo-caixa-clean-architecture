namespace FluxoCaixa.API.Controllers;

using Application.DTOs;
using Application.UseCases;
using Application.Validators;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class LancamentosController : ControllerBase
{
    private readonly CriarLancamentoUseCase _criarLancamentoUseCase;
    private readonly ObterLancamentosUseCase _obterLancamentosUseCase;
    private readonly ILogger<LancamentosController> _logger;

    public LancamentosController(
        CriarLancamentoUseCase criarLancamentoUseCase,
        ObterLancamentosUseCase obterLancamentosUseCase,
        ILogger<LancamentosController> logger)
    {
        _criarLancamentoUseCase = criarLancamentoUseCase;
        _obterLancamentosUseCase = obterLancamentosUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Cria um novo lançamento (débito ou crédito)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(LancamentoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LancamentoDto>> Criar([FromBody] CriarLancamentoDto dto)
    {
        try
        {
            var validator = new CriarLancamentoDtoValidator();
            var validationResult = await validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var resultado = await _criarLancamentoUseCase.ExecutarAsync(dto);
            _logger.LogInformation("Lançamento criado com sucesso. Id: {Id}", resultado.Id);

            return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar lançamento");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtém todos os lançamentos com paginação
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResultadoPaginado<LancamentoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultadoPaginado<LancamentoDto>>> ObterTodos(
        [FromQuery] int pagina = 1, 
        [FromQuery] int tamanhoPagina = 10)
    {
        var resultado = await _obterLancamentosUseCase.ExecutarAsync(pagina, tamanhoPagina);
        return Ok(resultado);
    }

    /// <summary>
    /// Obtém um lançamento específico por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LancamentoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LancamentoDto>> ObterPorId(Guid id)
    {
        var resultado = await _obterLancamentosUseCase.ExecutarPorIdAsync(id);
        
        if (resultado == null)
        {
            return NotFound();
        }

        return Ok(resultado);
    }
}
