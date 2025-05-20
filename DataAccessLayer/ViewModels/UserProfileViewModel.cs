using System.ComponentModel.DataAnnotations;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Http;

namespace DataAccessLayer.ViewModels;

public class UserProfileViewModel
{

    public string? Email { get; set; }
    [Required]
    [RegularExpression(@"^\S+$", ErrorMessage = "No whiteSpaces are allowed")]
    public string? Username { get; set; }

    [Required]
    [RegularExpression(@"^\S+$", ErrorMessage = "No whiteSpaces are allowed")]
    public string? Firstname { get; set; }

    [Required]
    [RegularExpression(@"^\S+$", ErrorMessage = "No whiteSpaces are allowed")]
    public string? Lastname { get; set; }

    public string? Profilepic { get; set; }
    public IFormFile? ImageUrl { get; set; }

    public int? Zipcode { get; set; }

    public string? Address { get; set; }

    [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$",
                       ErrorMessage = "Entered phone format is not valid.")]
    public string? Phone { get; set; }

    public int Countryid { get; set; }

    public int Stateid { get; set; }

    public int Cityid { get; set; }

    public List<Country>? CountryList { get; set; }
    public List<State>? StateList { get; set; }
    public List<City>? CityList { get; set; }
}
public class ChangePasswordViewModel
{

    [Required]
    public string OldPassword { get; set; }

    [Required]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$", ErrorMessage = "Password must be between 6 and 20 characters and contain one uppercase letter, one lowercase letter, one digit and one special character.")]
    public string NewPassword { get; set; } = null!;

    [Required]
    [Compare("NewPassword")]
    public string ConfirmNewPassword { get; set; } = null!;



}