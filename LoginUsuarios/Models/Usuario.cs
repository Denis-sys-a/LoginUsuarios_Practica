namespace LoginUsuarios.Models
{
    // Modelo que representa un usuario de la base de datos
    public class Usuario
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimoAcceso { get; set; }
    }
}
