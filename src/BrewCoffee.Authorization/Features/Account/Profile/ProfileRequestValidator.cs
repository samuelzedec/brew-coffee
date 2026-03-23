using FluentValidation;

namespace BrewCoffee.Authorization.Features.Account.Profile;

internal sealed class ProfileRequestValidator
    : AbstractValidator<ProfileRequest>
{
    public ProfileRequestValidator()
    {
        RuleFor(x => x.NewUsername)
            .MinimumLength(3)
            .WithMessage("O nome de usuário deve ter pelo menos 3 caracteres.")
            .MaximumLength(256)
            .WithMessage("O nome de usuário deve ter no máximo 256 caracteres.")
            .Matches("^[a-zA-Z0-9._-]+$")
            .WithMessage("O nome de usuário deve conter apenas letras, números, pontos, hífens e underscores.")
            .When(x => !string.IsNullOrEmpty(x.NewUsername));

        RuleFor(x => x.NewEmail)
            .EmailAddress()
            .WithMessage("O email informado não é válido.")
            .MaximumLength(256)
            .WithMessage("O email deve ter no máximo 256 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.NewEmail));
    }
}