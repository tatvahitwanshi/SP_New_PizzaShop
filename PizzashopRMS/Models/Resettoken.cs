using System;
using System.Collections.Generic;

namespace PizzashopRMS.Models;

public partial class Resettoken
{
    public int Id { get; set; }

    public string? Token { get; set; }

    public DateTime? Createdtime { get; set; }
}
