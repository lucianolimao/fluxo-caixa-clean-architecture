namespace FluxoCaixa.Domain.Tests.Entities;

using Domain.Entities;
using Enums;
using FluentAssertions;
using Xunit;

public class LancamentoTests
{
    [Fact]
    public void Criar_Lancamento_Credito_DeveRetornarSucesso()
    {
        var tipo = TipoLancamento.Credito;
        var valor = 100.50m;
        var descricao = "Venda de produto";
        var data = DateTime.UtcNow;
        
        var lancamento = new Lancamento(tipo, valor, descricao, data);
        
        lancamento.Should().NotBeNull();
        lancamento.Id.Should().NotBeEmpty();
        lancamento.Tipo.Should().Be(tipo);
        lancamento.Valor.Should().Be(valor);
        lancamento.Descricao.Should().Be(descricao);
        lancamento.Data.Should().Be(data);
    }

    [Fact]
    public void Criar_Lancamento_Debito_DeveRetornarSucesso()
    {
        var tipo = TipoLancamento.Debito;
        var valor = 50.00m;
        var descricao = "Pagamento de fornecedor";
        var data = DateTime.UtcNow;
        
        var lancamento = new Lancamento(tipo, valor, descricao, data);
        
        lancamento.Should().NotBeNull();
        lancamento.Tipo.Should().Be(tipo);
        lancamento.Valor.Should().Be(valor);
    }

    [Fact]
    public void Criar_Lancamento_ComValorZero_DeveLancarExcecao()
    {
        var tipo = TipoLancamento.Credito;
        var valor = 0m;
        var descricao = "Teste";
        var data = DateTime.UtcNow;
        
        Action act = () => new Lancamento(tipo, valor, descricao, data);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Valor deve ser maior que zero");
    }

    [Fact]
    public void Criar_Lancamento_ComValorNegativo_DeveLancarExcecao()
    {
        var tipo = TipoLancamento.Credito;
        var valor = -10m;
        var descricao = "Teste";
        var data = DateTime.UtcNow;
        
        Action act = () => new Lancamento(tipo, valor, descricao, data);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Valor deve ser maior que zero");
    }

    [Fact]
    public void Criar_Lancamento_SemDescricao_DeveLancarExcecao()
    {
        var tipo = TipoLancamento.Credito;
        var valor = 100m;
        var descricao = "";
        var data = DateTime.UtcNow;
        
        Action act = () => new Lancamento(tipo, valor, descricao, data);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Descrição é obrigatória");
    }

    [Fact]
    public void Criar_Lancamento_ComDescricaoMuitoLonga_DeveLancarExcecao()
    {
        var tipo = TipoLancamento.Credito;
        var valor = 100m;
        var descricao = new string('A', 501);
        var data = DateTime.UtcNow;
        
        Action act = () => new Lancamento(tipo, valor, descricao, data);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Descrição não pode ter mais de 500 caracteres");
    }

    [Fact]
    public void Criar_Lancamento_ComDataFutura_DeveLancarExcecao()
    {
        var tipo = TipoLancamento.Credito;
        var valor = 100m;
        var descricao = "Teste";
        var data = DateTime.UtcNow.AddDays(1);
        
        Action act = () => new Lancamento(tipo, valor, descricao, data);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Data do lançamento não pode ser futura");
    }

    [Theory]
    [InlineData(TipoLancamento.Credito, 100, 100)]
    [InlineData(TipoLancamento.Debito, 100, -100)]
    public void ObterValorComSinal_DeveRetornarValorCorreto(TipoLancamento tipo, decimal valor, decimal valorEsperado)
    {
        var lancamento = new Lancamento(tipo, valor, "Teste", DateTime.UtcNow);
        
        var resultado = lancamento.ObterValorComSinal();
        
        resultado.Should().Be(valorEsperado);
    }
}
