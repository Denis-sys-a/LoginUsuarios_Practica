using LoginUsuarios.Models;

namespace LoginUsuarios.Services
{
    public interface IAuthService
    {
        Task<Usuario?> ValidarUsuarioAsync(string username, string password);
        Task<RegistroResultado> RegistrarUsuarioAsync(string nombre, string username, string email, string password);
    }

    // Clase resultado del registro
    public class RegistroResultado
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public Usuario? Usuario { get; set; }
    }
}
