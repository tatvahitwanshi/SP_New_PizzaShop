using System.ComponentModel.DataAnnotations;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Http;

namespace DataAccessLayer.ViewModels;

public class EditUserViewModel
{
    public int? UserId { get; set; }
    [Required]
    public string? Email { get; set; }
    [Required]
    public string? Username { get; set; }

    [Required]
    public string? Firstname { get; set; }

    [Required]
    public string? Lastname { get; set; }
    public bool? Isactive { get; set; }
    public IFormFile? Profilepic { get; set; }

    [Required(ErrorMessage = "Zip Code is required")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Zip Code must be exactly 6 digits.")]
    public int? Zipcode { get; set; }


    public string? Address { get; set; }

    [Required]
    [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$",
                       ErrorMessage = "Entered phone format is not valid.")]
    public string? Phone { get; set; }
    [Required]
    public int Countryid { get; set; }
    [Required]
    public int Stateid { get; set; }
    [Required]
    public int Cityid { get; set; }
    [Required]
    public int Roleid { get; set; }
    public string? EditedBy { get; set; }
    public DateTime? EditDate { get; set; }
    public List<Role> Roles { get; set; } = new List<Role>();
    public List<Country> Countries { get; set; } = new List<Country>();
    public List<State> States { get; set; } = new List<State>();
    public List<City> Cities { get; set; } = new List<City>();
}
