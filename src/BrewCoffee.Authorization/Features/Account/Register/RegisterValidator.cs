using FluentValidation;

namespace BrewCoffee.Authorization.Features.Account.Register;

internal sealed class RegisterValidator
    : AbstractValidator<RegisterRequest>
{
    public RegisterValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithMessage("O nome de usuário é obrigatório.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("O e-mail é obrigatório.")
            .EmailAddress()
            .WithMessage("E-mail inválido.");

        RuleFor(x => x.Password)
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
            .WithMessage("A senha deve conter pelo menos um caractere especial.");
    }
}