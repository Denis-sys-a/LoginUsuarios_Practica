using LoginUsuarios.Models;

namespace LoginUsuarios.Services
{
    public class IAuthService
    {
        Task<Usuario?> ValidarUsuarioAsync(string username, string password);
    }
}
