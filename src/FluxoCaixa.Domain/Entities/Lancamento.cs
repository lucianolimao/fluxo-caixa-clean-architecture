namespace FluxoCaixa.Domain.Entities;

using Enums;

public class Lancamento
{
    public Guid Id { get; private set; }
    public TipoLancamento Tipo { get; private set; }
    public decimal Valor { get; private set; }
    public string Descricao { get; private set; }
    public DateTime Data { get; private set; }
    public DateTime DataCriacao { get; private set; }
    
    protected Lancamento() { }

    public Lancamento(TipoLancamento tipo, decimal valor, string descricao, DateTime data)
    {
        Id = Guid.NewGuid();
        Tipo = tipo;
        Descricao = descricao;
        Data = data;
        DataCriacao = DateTime.UtcNow;

        AtualizarValor(valor);
        Validar();
    }

    private void AtualizarValor(decimal valor)
    {
        if (valor <= 0)
            throw new ArgumentException("Valor deve ser maior que zero");

        Valor = valor;
    }

    private void Validar()
    {
        if (string.IsNullOrWhiteSpace(Descricao))
            throw new ArgumentException("Descrição é obrigatória");

        if (Descricao.Length > 500)
            throw new ArgumentException("Descrição não pode ter mais de 500 caracteres");

        if (Data > DateTime.UtcNow)
            throw new ArgumentException("Data do lançamento não pode ser futura");
    }

    public decimal ObterValorComSinal()
    {
        return Tipo == TipoLancamento.Credito ? Valor : -Valor;
    }
}
