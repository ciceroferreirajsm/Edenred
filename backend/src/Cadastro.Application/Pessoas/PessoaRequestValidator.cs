using Cadastro.Domain.Validation;
using FluentValidation;

namespace Cadastro.Application.Pessoas;

public sealed class PessoaRequestValidator : AbstractValidator<PessoaRequest>
{
    public const int NomeTamanhoMaximo = 150;
    public const int EmailTamanhoMaximo = 200;

    // Formato pragmático: usuario@dominio.tld, sem espaços e com TLD de 2+ caracteres.
    private const string EmailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s.]{2,}$";

    public PessoaRequestValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.Nome)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(NomeTamanhoMaximo).WithMessage($"Nome deve ter no máximo {NomeTamanhoMaximo} caracteres.");

        RuleFor(x => x.Cpf)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("CPF é obrigatório.")
            .Must(Cpf.EhValido).WithMessage("CPF inválido.");

        RuleFor(x => x.DataNascimento)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Data de nascimento é obrigatória.")
            .Must(d => d <= DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime))
                .WithMessage("Data de nascimento não pode estar no futuro.")
            .Must(d => d.Year >= 1900).WithMessage("Data de nascimento deve ser a partir de 1900.");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .MaximumLength(EmailTamanhoMaximo).WithMessage($"E-mail deve ter no máximo {EmailTamanhoMaximo} caracteres.")
            .Matches(EmailRegex).WithMessage("E-mail em formato inválido.");

        RuleFor(x => x.Telefone)
            .Must(TelefoneValido).WithMessage("Telefone deve conter DDD + número (10 ou 11 dígitos).")
            .When(x => !string.IsNullOrWhiteSpace(x.Telefone));
    }

    private static bool TelefoneValido(string? telefone)
    {
        if (telefone is null) return true;

        var digitos = telefone.Count(char.IsDigit);
        return digitos is 10 or 11 &&
               telefone.All(c => char.IsDigit(c) || c is ' ' or '(' or ')' or '-');
    }
}
