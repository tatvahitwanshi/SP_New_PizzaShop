using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DataAccessLayer.ViewModels;

public class UserListViewModel
{
    private object value;

    public int Userid { get; set; }

    [Required]
    [RegularExpression(@"^\S+$", ErrorMessage = "No whiteSpaces are allowed")]
    public string? Firstname { get; set;}
    
    [Required]
    [RegularExpression(@"^\S+$", ErrorMessage = "No whiteSpaces are allowed")]
    public string? Lastname {get; set;}

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? RoleName {get; set; }

    public bool? Isactive { get; set; }
    public bool? Isdeleted { get; set; }
    
    public string? Profilepic {get; set;}
}
