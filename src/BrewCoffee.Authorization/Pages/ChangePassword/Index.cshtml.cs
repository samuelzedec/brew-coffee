using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
using BrewCoffee.Shared.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrewCoffee.Authorization.Pages.ChangePassword;

[Authorize]
internal sealed class IndexModel(
    UserManager<ApplicationUser> userManager,
    ICurrentUserService currentUserService)
    : PageModel
{
    [BindProperty] public string CurrentPassword { get; set; } = string.Empty;
    [BindProperty] public string NewPassword { get; set; } = string.Empty;
    [BindProperty] public string ConfirmPassword { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (NewPassword != ConfirmPassword)
        {
            ErrorMessage = "As senhas não coincidem";
            return Page();
        }

        var user = await userManager.FindByIdAsync(currentUserService.Id);
        if (user is null) return Forbid();

        var result = await userManager.ChangePasswordAsync(
            user, CurrentPassword, NewPassword);

        if (result.Succeeded)
        {
            SuccessMessage = "Senha alterada com sucesso";
            return Page();
        }

        ErrorMessage = result.Errors.Any(e => e.Code == "PasswordMismatch")
            ? "Senha atual incorreta"
            : "Não foi possível alterar a senha";

        return Page();
    }
}