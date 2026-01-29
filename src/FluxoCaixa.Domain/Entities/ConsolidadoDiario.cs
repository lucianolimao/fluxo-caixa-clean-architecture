namespace FluxoCaixa.Domain.Entities;

public class ConsolidadoDiario
{
    public Guid Id { get; private set; }
    public DateTime Data { get; private set; }
    public decimal TotalCreditos { get; private set; }
    public decimal TotalDebitos { get; private set; }
    public decimal SaldoFinal { get; private set; }
    public int QuantidadeLancamentos { get; private set; }
    public DateTime UltimaAtualizacao { get; private set; }
    
    protected ConsolidadoDiario() { }

    public ConsolidadoDiario(DateTime data)
    {
        Id = Guid.NewGuid();
        Data = data;
        TotalCreditos = 0;
        TotalDebitos = 0;
        SaldoFinal = 0;
        QuantidadeLancamentos = 0;
        UltimaAtualizacao = DateTime.UtcNow;
    }

    public void AdicionarLancamento(Lancamento lancamento)
    {
        if (lancamento.Tipo == Enums.TipoLancamento.Credito)
        {
            TotalCreditos += lancamento.Valor;
        }
        else
        {
            TotalDebitos += lancamento.Valor;
        }

        QuantidadeLancamentos++;
        RecalcularSaldo();
        UltimaAtualizacao = DateTime.UtcNow;
    }

    private void RecalcularSaldo()
    {
        SaldoFinal = TotalCreditos - TotalDebitos;
    }

    public void Recalcular(IEnumerable<Lancamento> lancamentos)
    {
        TotalCreditos = 0;
        TotalDebitos = 0;
        QuantidadeLancamentos = 0;

        foreach (var lancamento in lancamentos)
        {
            if (lancamento.Tipo == Enums.TipoLancamento.Credito)
            {
                TotalCreditos += lancamento.Valor;
            }
            else
            {
                TotalDebitos += lancamento.Valor;
            }

            QuantidadeLancamentos++;
        }

        RecalcularSaldo();
        UltimaAtualizacao = DateTime.UtcNow;
    }
}
