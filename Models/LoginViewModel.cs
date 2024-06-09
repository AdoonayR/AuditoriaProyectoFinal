using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Required(ErrorMessage = "El número de empleado es obligatorio")]
    [RegularExpression(@"\d+", ErrorMessage = "El número de empleado debe contener solo números")]
    public string EmployeeNumber { get; set; }
}
