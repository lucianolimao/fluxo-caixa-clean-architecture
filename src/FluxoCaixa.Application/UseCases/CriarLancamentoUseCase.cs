namespace FluxoCaixa.Application.UseCases;

using Domain.Entities;
using Domain.Interfaces;
using DTOs;
using Interfaces;

public class CriarLancamentoUseCase
{
    private readonly ILancamentoRepository _lancamentoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessagePublisher _messagePublisher;

    public CriarLancamentoUseCase(
        ILancamentoRepository lancamentoRepository,
        IUnitOfWork unitOfWork,
        IMessagePublisher messagePublisher)
    {
        _lancamentoRepository = lancamentoRepository;
        _unitOfWork = unitOfWork;
        _messagePublisher = messagePublisher;
    }

    public async Task<LancamentoDto> ExecutarAsync(CriarLancamentoDto dto)
    {
        var lancamento = new Lancamento(
            dto.Tipo,
            dto.Valor,
            dto.Descricao,
            dto.Data
        );

        await _lancamentoRepository.AdicionarAsync(lancamento);
        await _unitOfWork.CommitAsync();
        
        await _messagePublisher.PublicarLancamentoCriadoAsync(lancamento.Id, lancamento.Data);

        return MapearParaDto(lancamento);
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
