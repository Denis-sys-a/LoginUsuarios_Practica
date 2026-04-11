using LoginUsuarios.Models;

namespace LoginUsuarios.Services
{
    public interface IAuthService
    {
        Task<Usuario?> ValidarUsuarioAsync(string username, string password);
    }
}
