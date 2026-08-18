using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sucursal360.Web.Data;

namespace Sucursal360.Web.Areas.Identity.Pages.Account;

public class LoginModel(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    ILogger<LoginModel> logger) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public async Task OnGetAsync()
    {
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ReturnUrl ??= Url.Content("~/");
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await userManager.FindByEmailAsync(Input.Email);
        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "No se pudo iniciar sesion con las credenciales indicadas.");
            return Page();
        }

        var result = await signInManager.PasswordSignInAsync(
            user.UserName!,
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            logger.LogInformation("Demo user signed in.");
            var targetUrl = Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : Url.Content("~/");
            return LocalRedirect(targetUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "La cuenta esta temporalmente bloqueada. Intente nuevamente mas tarde.");
            return Page();
        }

        ModelState.AddModelError(string.Empty, "No se pudo iniciar sesion con las credenciales indicadas.");
        return Page();
    }

    public sealed class InputModel
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo valido.")]
        [Display(Name = "Correo")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contrasena es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contrasena")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Mantener sesion iniciada")]
        public bool RememberMe { get; set; }
    }
}
