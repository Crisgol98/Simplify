using System.ComponentModel.DataAnnotations;

namespace Simplify.Models
{
    public class UserAccount
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Por favor, introduce tu nombre de usuario.")]
        public string? Username { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        [Required(ErrorMessage = "Por favor, introduce tu contraseña.")]
        public string? Password { get; set; }
    }
}
