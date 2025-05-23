using System;
using System.Collections.Generic;

namespace PizzashopRMS.Models;

public partial class Country
{
    public int Countryid { get; set; }

    public string Countryname { get; set; } = null!;

    public virtual ICollection<State> States { get; } = new List<State>();
}
