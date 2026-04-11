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

        // ─── LOGIN ────────────────────────────────────────────────────────────
        public async Task<Usuario?> ValidarUsuarioAsync(string username, string password)
        {
            string hash = HashPassword(password);

            // Verificar estado antes de abrir
            if (_conn.State != ConnectionState.Open)
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

                // Actualizar ultimo acceso
                string updateSql = "UPDATE usuarios SET ultimo_acceso = NOW() WHERE id = @id";
                using var updateCmd = new MySqlCommand(updateSql, _conn);
                updateCmd.Parameters.AddWithValue("@id", usuario.Id);
                await updateCmd.ExecuteNonQueryAsync();

                return usuario;
            }

            return null;
        }

        // ─── REGISTRO ─────────────────────────────────────────────────────────
        public async Task<RegistroResultado> RegistrarUsuarioAsync(string nombre, string username, string email, string password)
        {
            if (_conn.State != ConnectionState.Open)
                await _conn.OpenAsync();

            // Verificar si el username ya existe
            string checkUser = "SELECT COUNT(*) FROM usuarios WHERE username = @username";
            using var cmdUser = new MySqlCommand(checkUser, _conn);
            cmdUser.Parameters.AddWithValue("@username", username);
            var countUser = Convert.ToInt32(await cmdUser.ExecuteScalarAsync());

            if (countUser > 0)
                return new RegistroResultado { Exito = false, Mensaje = "El nombre de usuario ya está en uso." };

            // Verificar si el email ya existe
            string checkEmail = "SELECT COUNT(*) FROM usuarios WHERE email = @email";
            using var cmdEmail = new MySqlCommand(checkEmail, _conn);
            cmdEmail.Parameters.AddWithValue("@email", email);
            var countEmail = Convert.ToInt32(await cmdEmail.ExecuteScalarAsync());

            if (countEmail > 0)
                return new RegistroResultado { Exito = false, Mensaje = "El correo electrónico ya está registrado." };

            // Insertar nuevo usuario
            string hash = HashPassword(password);

            string insertSql = @"INSERT INTO usuarios (nombre, username, password_hash, email, activo)
                                 VALUES (@nombre, @username, @hash, @email, 1);
                                 SELECT LAST_INSERT_ID();";

            using var cmdInsert = new MySqlCommand(insertSql, _conn);
            cmdInsert.Parameters.AddWithValue("@nombre", nombre);
            cmdInsert.Parameters.AddWithValue("@username", username);
            cmdInsert.Parameters.AddWithValue("@hash", hash);
            cmdInsert.Parameters.AddWithValue("@email", email);

            var newId = Convert.ToInt32(await cmdInsert.ExecuteScalarAsync());

            var usuario = new Usuario
            {
                Id = newId,
                Username = username,
                Email = email,
                Activo = true,
                FechaRegistro = DateTime.Now
            };

            return new RegistroResultado
            {
                Exito = true,
                Mensaje = "Registro exitoso.",
                Usuario = usuario
            };
        }

        // ─── HASH ─────────────────────────────────────────────────────────────
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}
