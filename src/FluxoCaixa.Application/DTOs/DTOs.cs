namespace FluxoCaixa.Application.DTOs;

using FluxoCaixa.Domain.Enums;

public class CriarLancamentoDto
{
    public TipoLancamento Tipo { get; set; }
    public decimal Valor { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime Data { get; set; }
}

public class LancamentoDto
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public DateTime DataCriacao { get; set; }
}

public class ConsolidadoDiarioDto
{
    public DateTime Data { get; set; }
    public decimal TotalCreditos { get; set; }
    public decimal TotalDebitos { get; set; }
    public decimal SaldoFinal { get; set; }
    public int QuantidadeLancamentos { get; set; }
    public DateTime UltimaAtualizacao { get; set; }
}

public class ResultadoPaginado<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int PaginaAtual { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalItens { get; set; }
}
