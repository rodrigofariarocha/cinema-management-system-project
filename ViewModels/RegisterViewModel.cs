using System.ComponentModel.DataAnnotations;

namespace RochaCinema.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "O nome completo é obrigatório")]
    [Display(Name = "Nome Completo")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A palavra-passe é obrigatória")]
    [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Palavra-passe")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirmar palavra-passe")]
    [Compare("Password", ErrorMessage = "A palavra-passe e a confirmação não coincidem.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
