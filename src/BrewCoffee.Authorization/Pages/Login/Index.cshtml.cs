using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrewCoffee.Authorization.Pages.Login;

internal sealed class IndexModel(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager)
    : PageModel
{
    [BindProperty] public string Email { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;
    [BindProperty] public string ReturnUrl { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public void OnGet(string? returnUrl)
        => ReturnUrl = returnUrl ?? "/";

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await userManager.FindByEmailAsync(Email);
        if (user is null)
        {
            ErrorMessage = "Usuário não encontrado.";
            return Page();
        }

        var result = await signInManager.PasswordSignInAsync(
            user.UserName!,
            Password,
            isPersistent: false,
            lockoutOnFailure: true);

        if (result.Succeeded)
            return Redirect(ReturnUrl);

        ErrorMessage = "Senha inválida";
        return Page();
    }
}