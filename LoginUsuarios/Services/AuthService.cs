using LoginUsuarios.Models;
using MySql.Data.MySqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace LoginUsuarios.Services
{
    public class AuthService : IAuthService
    {
        // Conexión a la base de datos MySQL
        private readonly MySqlConnection _conn;
        public AuthService(MySqlConnection conn) { _conn = conn; }

        // ─── VALIDAR LOGIN ────────────────────────────────────────────────────
        public async Task<Usuario?> ValidarUsuarioAsync(string username, string password)
        {
            string hash = HashPassword(password);

            // Abrir conexión solo si no está abierta
            if (_conn.State != ConnectionState.Open) await _conn.OpenAsync();

            // Buscar usuario que coincida con username, hash y que esté activo
            string sql = @"SELECT id, username, email, activo, fecha_registro, ultimo_acceso
                           FROM usuarios WHERE username = @username AND password_hash = @hash AND activo = 1";
            using var cmd = new MySqlCommand(sql, _conn);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@hash", hash);
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                // Mapear los datos del resultado a un objeto Usuario
                var usuario = new Usuario
                {
                    Id = reader.GetInt32("id"),
                    Username = reader.GetString("username"),
                    Email = reader.GetString("email"),
                    Activo = reader.GetBoolean("activo"),
                    FechaRegistro = reader.GetDateTime("fecha_registro"),
                    // Si ultimo_acceso es NULL en la BD, asignar null al objeto
                    UltimoAcceso = reader.IsDBNull(reader.GetOrdinal("ultimo_acceso")) ? null : reader.GetDateTime("ultimo_acceso")
                };
                await reader.CloseAsync();
                // Registrar la fecha y hora del último acceso exitoso
                using var upd = new MySqlCommand("UPDATE usuarios SET ultimo_acceso = NOW() WHERE id = @id", _conn);
                upd.Parameters.AddWithValue("@id", usuario.Id);
                await upd.ExecuteNonQueryAsync();
                return usuario;
            }
            return null;
        }

        // ─── REGISTRAR USUARIO ────────────────────────────────────────────────
        public async Task<RegistroResultado> RegistrarUsuarioAsync(string nombre, string username, string email, string password)
        {
            if (_conn.State != ConnectionState.Open) await _conn.OpenAsync();

            // Verificar que el nombre de usuario no esté ya en uso
            using var c1 = new MySqlCommand("SELECT COUNT(*) FROM usuarios WHERE username = @u", _conn);
            c1.Parameters.AddWithValue("@u", username);
            if (Convert.ToInt32(await c1.ExecuteScalarAsync()) > 0)
                return new RegistroResultado { Exito = false, Mensaje = "El nombre de usuario ya esta en uso." };

            // Verificar que el correo electrónico no esté ya registrado
            using var c2 = new MySqlCommand("SELECT COUNT(*) FROM usuarios WHERE email = @e", _conn);
            c2.Parameters.AddWithValue("@e", email);
            if (Convert.ToInt32(await c2.ExecuteScalarAsync()) > 0)
                return new RegistroResultado { Exito = false, Mensaje = "El correo electronico ya esta registrado." };

            // Hashear la contraseña antes de guardarla en la BD
            string hash = HashPassword(password);

            string sql = @"INSERT INTO usuarios (nombre, username, password_hash, email, activo)
                           VALUES (@nombre, @username, @hash, @email, 1); SELECT LAST_INSERT_ID();";
            using var ins = new MySqlCommand(sql, _conn);
            ins.Parameters.AddWithValue("@nombre", nombre);
            ins.Parameters.AddWithValue("@username", username);
            ins.Parameters.AddWithValue("@hash", hash);
            ins.Parameters.AddWithValue("@email", email);
            var newId = Convert.ToInt32(await ins.ExecuteScalarAsync());

            return new RegistroResultado
            {
                Exito = true,
                Mensaje = "Registro exitoso.",
                Usuario = new Usuario { Id = newId, Username = username, Email = email, Activo = true, FechaRegistro = DateTime.Now }
            };
        }

        // ─── CAMBIAR CONTRASEÑA ───────────────────────────────────────────────
        public async Task<bool> CambiarPasswordAsync(int userId, string passwordActual, string passwordNueva)
        {
            if (_conn.State != ConnectionState.Open) await _conn.OpenAsync();

            // Verificar que la contraseña actual ingresada sea correcta
            string hashActual = HashPassword(passwordActual);
            using var check = new MySqlCommand("SELECT COUNT(*) FROM usuarios WHERE id = @id AND password_hash = @hash", _conn);
            check.Parameters.AddWithValue("@id", userId);
            check.Parameters.AddWithValue("@hash", hashActual);

            // Si no coincide, retornar false sin hacer ningún cambio
            if (Convert.ToInt32(await check.ExecuteScalarAsync()) == 0) return false;

            // Hashear la nueva contraseña y actualizarla en la BD
            string hashNueva = HashPassword(passwordNueva);
            using var upd = new MySqlCommand("UPDATE usuarios SET password_hash = @hash WHERE id = @id", _conn);
            upd.Parameters.AddWithValue("@hash", hashNueva);
            upd.Parameters.AddWithValue("@id", userId);
            await upd.ExecuteNonQueryAsync();
            return true;
        }

        // ─── HASH SHA-256 ─────────────────────────────────────────────────────
        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(password))).ToLower();
        }
    }
}
