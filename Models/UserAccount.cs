using System.ComponentModel.DataAnnotations;

namespace Simplify.Models
{
    public class UserAccount
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(30, ErrorMessage = "El nombre no puede tener más de 30 caracteres")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(30, ErrorMessage = "El nombre de usuario no puede tener más de 30 caracteres")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [RegularExpression(@"^(?=.*[0-9]).{6,}$",
            ErrorMessage = "La contraseña debe tener al menos 6 caracteres y contener al menos un número")]
        public string? Password { get; set; }
    }

}
