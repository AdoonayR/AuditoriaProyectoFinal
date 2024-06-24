using System.ComponentModel.DataAnnotations;

// Modelo de vista para el formulario de inicio de sesión
public class LoginViewModel
{
    // Propiedad que representa el número de empleado ingresado por el usuario
    [Required(ErrorMessage = "El número de empleado es obligatorio")]
    // Valida que el número de empleado solo contenga dígitos
    [RegularExpression(@"\d+", ErrorMessage = "El número de empleado debe contener solo números")]
    public string EmployeeNumber { get; set; }
}
