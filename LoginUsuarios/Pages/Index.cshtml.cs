using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LoginUsuarios.Pages
{
    public class IndexModel : PageModel
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool RegistroExitoso { get; set; } = false;

        public IActionResult OnGet()
        {
            var sessionUsername = HttpContext.Session.GetString("Username");

            // Si no hay sesion activa, ir al Login
            if (string.IsNullOrEmpty(sessionUsername))
                return RedirectToPage("/Account/Login");

            Username = sessionUsername;
            Email = HttpContext.Session.GetString("Email") ?? string.Empty;

            // Mostrar banner de registro exitoso si aplica
            RegistroExitoso = HttpContext.Session.GetString("RegistroExitoso") == "true";
            if (RegistroExitoso)
                HttpContext.Session.Remove("RegistroExitoso");

            return Page();
        }
    }
}
