namespace FluxoCaixa.Application.Validators;

using FluentValidation;
using DTOs;

public class CriarLancamentoDtoValidator : AbstractValidator<CriarLancamentoDto>
{
    public CriarLancamentoDtoValidator()
    {
        RuleFor(x => x.Valor)
            .GreaterThan(0)
            .WithMessage("Valor deve ser maior que zero");

        RuleFor(x => x.Descricao)
            .NotEmpty()
            .WithMessage("Descrição é obrigatória")
            .MaximumLength(500)
            .WithMessage("Descrição não pode ter mais de 500 caracteres");

        RuleFor(x => x.Data)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Data do lançamento não pode ser futura");

        RuleFor(x => x.Tipo)
            .IsInEnum()
            .WithMessage("Tipo de lançamento inválido");
    }
}
