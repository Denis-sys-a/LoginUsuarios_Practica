using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LoginUsuarios.Services;

namespace LoginUsuarios.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;

        public LoginModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
           
            if (HttpContext.Session.GetString("Username") != null)
                return RedirectToPage("/Index");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Ingresa tu usuario y contraseña.";
                return Page();
            }

            var usuario = await _authService.ValidarUsuarioAsync(Username, Password);

            if (usuario == null)
            {
                ErrorMessage = "Usuario o contraseña incorrectos.";
                return Page();
            }

            
            HttpContext.Session.SetInt32("UserId", usuario.Id);
            HttpContext.Session.SetString("Username", usuario.Username);
            HttpContext.Session.SetString("Email", usuario.Email);

            return RedirectToPage("/Index");
        }
    }
}
