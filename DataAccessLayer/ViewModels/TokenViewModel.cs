namespace DataAccessLayer.ViewModels;

public class TokenViewModel
{
    public string Roleid {get; set;}
    public string Email{get; set;}

    public string Username { get; set; } = null!;

    public bool valid {get; set;}
}
