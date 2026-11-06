using Cadastro.Domain.Entities;
using Cadastro.Domain.Validation;
using FluentValidation;

namespace Cadastro.Application.Veiculos;

public sealed class VeiculoRequestValidator : AbstractValidator<VeiculoRequest>
{
    public const int MarcaTamanhoMaximo = 60;
    public const int ModeloTamanhoMaximo = 80;

    public VeiculoRequestValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.Placa)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Placa é obrigatória.")
            .Must(Placa.EhValida).WithMessage("Placa inválida. Use o formato ABC1234 ou ABC1D23 (Mercosul).");

        RuleFor(x => x.AnoFabricacao)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(Veiculo.AnoMinimoFabricacao)
                .WithMessage($"Ano de fabricação deve ser maior ou igual a {Veiculo.AnoMinimoFabricacao}.")
            .Must(ano => ano <= timeProvider.GetLocalNow().Year)
                .WithMessage(_ => $"Ano de fabricação não pode ser maior que o ano atual ({timeProvider.GetLocalNow().Year}).");

        RuleFor(x => x.Marca)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Marca é obrigatória.")
            .MaximumLength(MarcaTamanhoMaximo).WithMessage($"Marca deve ter no máximo {MarcaTamanhoMaximo} caracteres.");

        RuleFor(x => x.Modelo)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Modelo é obrigatório.")
            .MaximumLength(ModeloTamanhoMaximo).WithMessage($"Modelo deve ter no máximo {ModeloTamanhoMaximo} caracteres.");

        RuleFor(x => x.PessoaId)
            .NotEmpty().WithMessage("Pessoa (proprietário) é obrigatória.");
    }
}

public sealed class VincularVeiculoRequestValidator : AbstractValidator<VincularVeiculoRequest>
{
    public VincularVeiculoRequestValidator()
    {
        RuleFor(x => x.PessoaId)
            .NotEmpty().WithMessage("Pessoa (proprietário) é obrigatória.");
    }
}
