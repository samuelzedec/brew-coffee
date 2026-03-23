using FluentValidation;

namespace BrewCoffee.Authorization.Features.Account.ChangePassword;

internal sealed class ChangePasswordRequestValidator
    : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("A senha atual é obrigatória");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("A senha é obrigatória.")
            .MinimumLength(8)
            .WithMessage("A senha deve ter pelo menos 8 caracteres.")
            .Matches("[A-Z]")
            .WithMessage("A senha deve conter pelo menos uma letra maiúscula.")
            .Matches("[a-z]")
            .WithMessage("A senha deve conter pelo menos uma letra minúscula.")
            .Matches("[0-9]")
            .WithMessage("A senha deve conter pelo menos um número.")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("A senha deve conter pelo menos um caractere especial.")
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("A nova senha não pode ser igual à senha atual.");
    }
}