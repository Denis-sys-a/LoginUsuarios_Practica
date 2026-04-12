using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LoginUsuarios.Services;

namespace LoginUsuarios.Pages.Account
{
    public class ChangePasswordModel : PageModel
    {
        private readonly IAuthService _authService;
        public ChangePasswordModel(IAuthService authService) { _authService = authService; }

        [BindProperty] public string PasswordActual { get; set; } = string.Empty;
        [BindProperty] public string PasswordNueva { get; set; } = string.Empty;
        [BindProperty] public string ConfirmarPassword { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToPage("/Account/Login");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToPage("/Account/Login");

            if (string.IsNullOrWhiteSpace(PasswordActual) ||
                string.IsNullOrWhiteSpace(PasswordNueva) ||
                string.IsNullOrWhiteSpace(ConfirmarPassword))
            {
                ErrorMessage = "Todos los campos son obligatorios.";
                return Page();
            }

            if (PasswordNueva != ConfirmarPassword)
            {
                ErrorMessage = "Las contraseñas nuevas no coinciden.";
                return Page();
            }

            if (PasswordNueva.Length < 6)
            {
                ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres.";
                return Page();
            }

            if (PasswordActual == PasswordNueva)
            {
                ErrorMessage = "La nueva contraseña debe ser diferente a la actual.";
                return Page();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                ErrorMessage = "Sesión inválida. Inicia sesión nuevamente.";
                return Page();
            }

            var resultado = await _authService.CambiarPasswordAsync(userId.Value, PasswordActual, PasswordNueva);

            if (!resultado)
            {
                ErrorMessage = "La contraseña actual es incorrecta.";
                return Page();
            }

            SuccessMessage = "Contraseña cambiada exitosamente.";
            return Page();
        }
    }
}
