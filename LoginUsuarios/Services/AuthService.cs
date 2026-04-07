using LoginUsuarios.Models;
using MySql.Data.MySqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace LoginUsuarios.Services
{
    public class AuthService : IAuthService
    {
        private readonly MySqlConnection _conn;

        public AuthService(MySqlConnection conn)
        {
            _conn = conn;
        }

        public async Task<Usuario?> ValidarUsuarioAsync(string username, string password)
        {
            string hash = HashPassword(password);

            await _conn.OpenAsync();

            string sql = @"SELECT id, username, email, activo, fecha_registro, ultimo_acceso
                           FROM usuarios
                           WHERE username = @username AND password_hash = @hash AND activo = 1";

            using var cmd = new MySqlCommand(sql, _conn);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@hash", hash);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var usuario = new Usuario
                {
                    Id = reader.GetInt32("id"),
                    Username = reader.GetString("username"),
                    Email = reader.GetString("email"),
                    Activo = reader.GetBoolean("activo"),
                    FechaRegistro = reader.GetDateTime("fecha_registro"),
                    UltimoAcceso = reader.IsDBNull(reader.GetOrdinal("ultimo_acceso"))
                                        ? null
                                        : reader.GetDateTime("ultimo_acceso")
                };

                await reader.CloseAsync();

                
                string updateSql = "UPDATE usuarios SET ultimo_acceso = NOW() WHERE id = @id";
                using var updateCmd = new MySqlCommand(updateSql, _conn);
                updateCmd.Parameters.AddWithValue("@id", usuario.Id);
                await updateCmd.ExecuteNonQueryAsync();

                return usuario;
            }

            return null;
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}
