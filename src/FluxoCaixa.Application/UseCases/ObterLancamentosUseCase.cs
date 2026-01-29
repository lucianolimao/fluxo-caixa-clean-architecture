namespace FluxoCaixa.Application.UseCases;

using Domain.Entities;
using Domain.Interfaces;
using DTOs;

public class ObterLancamentosUseCase
{
    private readonly ILancamentoRepository _lancamentoRepository;

    public ObterLancamentosUseCase(ILancamentoRepository lancamentoRepository)
    {
        _lancamentoRepository = lancamentoRepository;
    }

    public async Task<ResultadoPaginado<LancamentoDto>> ExecutarAsync(int pagina = 1, int tamanhoPagina = 10)
    {
        var lancamentos = await _lancamentoRepository.ObterTodosAsync(pagina, tamanhoPagina);
        var totalItens = await _lancamentoRepository.ContarAsync();

        return new ResultadoPaginado<LancamentoDto>
        {
            Items = lancamentos.Select(MapearParaDto),
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalItens = totalItens
        };
    }

    public async Task<LancamentoDto?> ExecutarPorIdAsync(Guid id)
    {
        var lancamento = await _lancamentoRepository.ObterPorIdAsync(id);
        return lancamento != null ? MapearParaDto(lancamento) : null;
    }

    private static LancamentoDto MapearParaDto(Lancamento lancamento)
    {
        return new LancamentoDto
        {
            Id = lancamento.Id,
            Tipo = lancamento.Tipo.ToString(),
            Valor = lancamento.Valor,
            Descricao = lancamento.Descricao,
            Data = lancamento.Data,
            DataCriacao = lancamento.DataCriacao
        };
    }
}
