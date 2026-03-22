using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrewCoffee.Authorization.Pages.Register;

internal sealed class IndexModel(
    UserManager<ApplicationUser> userManager)
    : PageModel
{
    [BindProperty] public string Email { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;
    [BindProperty] public string ConfirmPassword { get; set; } = string.Empty;
    [BindProperty] public string ReturnUrl { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public void OnGet(string? returnUrl)
        => ReturnUrl = returnUrl ?? "/";

    public async Task<IActionResult> OnPostAsync()
    {
        var validationError = ValidateInput();
        if (validationError is not null)
        {
            ErrorMessage = validationError;
            return Page();
        }

        if (await EmailAlreadyExistsAsync())
        {
            ErrorMessage = "Já existe uma conta com esse e-mail";
            return Page();
        }

        var result = await CreateUserAsync();
        if (!result.Succeeded)
        {
            ErrorMessage = "Não foi possível criar a conta. Tente novamente";
            return Page();
        }

        return Redirect($"/login?returnUrl={Uri.EscapeDataString(ReturnUrl)}");
    }

    private string? ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(Email) || !Email.Contains('@'))
            return "E-mail inválido";

        if (Password != ConfirmPassword)
            return "As senhas não coincidem";

        return !IsPasswordValid()
            ? "A senha deve ter pelo menos 8 caracteres, uma maiúscula, uma minúscula, um número e um caractere especial"
            : null;
    }

    private bool IsPasswordValid() =>
        Password.Length >= 8 &&
        Password.Any(char.IsUpper) &&
        Password.Any(char.IsLower) &&
        Password.Any(char.IsDigit) &&
        Password.Any(c => !char.IsLetterOrDigit(c));

    private async Task<bool> EmailAlreadyExistsAsync() =>
        await userManager.FindByEmailAsync(Email) is not null;

    private async Task<IdentityResult> CreateUserAsync()
    {
        var user = new ApplicationUser { UserName = Email[..Email.IndexOf('@')], Email = Email, EmailConfirmed = true };

        return await userManager.CreateAsync(user, Password);
    }
}