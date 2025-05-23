using System;
using System.Collections.Generic;

namespace PizzashopRMS.Models;

public partial class User
{
    public int Userid { get; set; }

    public string Username { get; set; } = null!;

    public string? Firstname { get; set; }

    public string? Lastname { get; set; }

    public string? Email { get; set; }

    public string Password { get; set; } = null!;

    public string? Profilepic { get; set; }

    public int? Zipcode { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public bool? Isactive { get; set; }

    public bool? Isdeleted { get; set; }

    public int Roleid { get; set; }

    public int Countryid { get; set; }

    public int Stateid { get; set; }

    public int Cityid { get; set; }

    public string? CreatedBy { get; set; }

    public string? EditedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? EditDate { get; set; }

    public virtual Role Role { get; set; } = null!;
}
