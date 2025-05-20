using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.ViewModels;

public class LoginViewModel
{
    [Required]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }

    public bool RememberMe { get; set; }

}

public class ForgetPasswordViewModel
{
    [Required]
    public string? Email { get; set; }
}

public class ResetPasswordViewModel
{ 
       
    [Required]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$", ErrorMessage = "Password must be between 6 and 20 characters and contain one uppercase letter, one lowercase letter, one digit and one special character.")]
    public string? Password { get; set; } = null;
    [Required]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string? ConfirmPassword { get; set; }= null;
    public string? Email { get; set; }



    
}


