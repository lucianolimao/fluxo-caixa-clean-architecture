namespace FluxoCaixa.Domain.Tests.Entities;

using Domain.Entities;
using Enums;
using FluentAssertions;
using Xunit;

public class ConsolidadoDiarioTests
{
    [Fact]
    public void Criar_ConsolidadoDiario_DeveInicializarComValoresZerados()
    {
        var data = new DateTime(2024, 1, 29);
        
        var consolidado = new ConsolidadoDiario(data);
        
        consolidado.Should().NotBeNull();
        consolidado.Id.Should().NotBeEmpty();
        consolidado.Data.Should().Be(data);
        consolidado.TotalCreditos.Should().Be(0);
        consolidado.TotalDebitos.Should().Be(0);
        consolidado.SaldoFinal.Should().Be(0);
        consolidado.QuantidadeLancamentos.Should().Be(0);
    }

    [Fact]
    public void AdicionarLancamento_Credito_DeveAtualizarTotais()
    {
        var data = new DateTime(2024, 1, 29);
        var consolidado = new ConsolidadoDiario(data);
        var lancamento = new Lancamento(
            TipoLancamento.Credito,
            100m,
            "Venda",
            new DateTime(2024, 1, 29)
        );
        
        consolidado.AdicionarLancamento(lancamento);
        
        consolidado.TotalCreditos.Should().Be(100m);
        consolidado.TotalDebitos.Should().Be(0);
        consolidado.SaldoFinal.Should().Be(100m);
        consolidado.QuantidadeLancamentos.Should().Be(1);
    }

    [Fact]
    public void AdicionarLancamento_Debito_DeveAtualizarTotais()
    {
        var data = new DateTime(2024, 1, 29);
        var consolidado = new ConsolidadoDiario(data);
        var lancamento = new Lancamento(
            TipoLancamento.Debito,
            50m,
            "Pagamento",
            new DateTime(2024, 1, 29)
        );
        
        consolidado.AdicionarLancamento(lancamento);
        
        consolidado.TotalCreditos.Should().Be(0);
        consolidado.TotalDebitos.Should().Be(50m);
        consolidado.SaldoFinal.Should().Be(-50m);
        consolidado.QuantidadeLancamentos.Should().Be(1);
    }

    [Fact]
    public void AdicionarMultiplosLancamentos_DeveCalcularSaldoCorreto()
    {
        var data = new DateTime(2024, 1, 29);
        var consolidado = new ConsolidadoDiario(data);
        
        var credito1 = new Lancamento(TipoLancamento.Credito, 100m, "Venda 1", new DateTime(2024, 1, 29));
        var credito2 = new Lancamento(TipoLancamento.Credito, 200m, "Venda 2", new DateTime(2024, 1, 29));
        var debito1 = new Lancamento(TipoLancamento.Debito, 50m, "Pagamento 1", new DateTime(2024, 1, 29));
        
        consolidado.AdicionarLancamento(credito1);
        consolidado.AdicionarLancamento(credito2);
        consolidado.AdicionarLancamento(debito1);
        
        consolidado.TotalCreditos.Should().Be(300m); // 100 + 200
        consolidado.TotalDebitos.Should().Be(50m);
        consolidado.SaldoFinal.Should().Be(250m); // 300 - 50
        consolidado.QuantidadeLancamentos.Should().Be(3);
    }

    [Fact]
    public void Recalcular_DeveAtualizarTodosOsValores()
    {
        var data = new DateTime(2024, 1, 29);
        var consolidado = new ConsolidadoDiario(data);
        
        var lancamentos = new List<Lancamento>
        {
            new Lancamento(TipoLancamento.Credito, 100m, "Venda 1", new DateTime(2024, 1, 29)),
            new Lancamento(TipoLancamento.Credito, 200m, "Venda 2", new DateTime(2024, 1, 29)),
            new Lancamento(TipoLancamento.Debito, 75m, "Pagamento", new DateTime(2024, 1, 29))
        };
        
        consolidado.Recalcular(lancamentos);
        
        consolidado.TotalCreditos.Should().Be(300m);
        consolidado.TotalDebitos.Should().Be(75m);
        consolidado.SaldoFinal.Should().Be(225m);
        consolidado.QuantidadeLancamentos.Should().Be(3);
    }

    [Fact]
    public void Recalcular_ComListaVazia_DeveZerarValores()
    {
        var data = new DateTime(2024, 1, 29);
        var consolidado = new ConsolidadoDiario(data);
        
        consolidado.AdicionarLancamento(new Lancamento(TipoLancamento.Credito, 100m, "Teste", new DateTime(2024, 1, 29)));
        
        var lancamentosVazios = new List<Lancamento>();
        
        consolidado.Recalcular(lancamentosVazios);
        
        consolidado.TotalCreditos.Should().Be(0);
        consolidado.TotalDebitos.Should().Be(0);
        consolidado.SaldoFinal.Should().Be(0);
        consolidado.QuantidadeLancamentos.Should().Be(0);
    }
}