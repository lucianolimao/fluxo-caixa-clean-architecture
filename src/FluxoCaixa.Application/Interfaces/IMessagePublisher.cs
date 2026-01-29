namespace FluxoCaixa.Application.Interfaces;

public interface IMessagePublisher
{
    Task PublicarLancamentoCriadoAsync(Guid lancamentoId, DateTime data);
}
