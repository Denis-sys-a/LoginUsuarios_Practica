using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LoginUsuarios.Services;

namespace LoginUsuarios.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;

        public RegisterModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public string Nombre { get; set; } = string.Empty;

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public string ConfirmarPassword { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public IActionResult OnGet()
        {
            // Si ya hay sesión activa, redirigir al Index
            if (HttpContext.Session.GetString("Username") != null)
                return RedirectToPage("/Index");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validar campos vacíos
            if (string.IsNullOrWhiteSpace(Nombre) ||
                string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(ConfirmarPassword))
            {
                ErrorMessage = "Todos los campos son obligatorios.";
                return Page();
            }

            // Validar que las contraseñas coincidan
            if (Password != ConfirmarPassword)
            {
                ErrorMessage = "Las contraseñas no coinciden.";
                return Page();
            }

            // Validar longitud mínima de contraseña
            if (Password.Length < 6)
            {
                ErrorMessage = "La contraseña debe tener al menos 6 caracteres.";
                return Page();
            }

            // Intentar registrar el usuario
            var resultado = await _authService.RegistrarUsuarioAsync(Nombre, Username, Email, Password);

            if (!resultado.Exito)
            {
                ErrorMessage = resultado.Mensaje;
                return Page();
            }

            // Registro exitoso: guardar sesión y redirigir al Index con mensaje
            HttpContext.Session.SetInt32("UserId", resultado.Usuario!.Id);
            HttpContext.Session.SetString("Username", resultado.Usuario.Username);
            HttpContext.Session.SetString("Email", resultado.Usuario.Email);
            HttpContext.Session.SetString("RegistroExitoso", "true");

            return RedirectToPage("/Index");
        }
    }
}
