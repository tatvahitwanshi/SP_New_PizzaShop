using System;
using System.Collections.Generic;

namespace PizzashopRMS.Models;

public partial class ItemsUnit
{
    public int Unitid { get; set; }

    public string Unitname { get; set; } = null!;

    public virtual ICollection<Item> Items { get; } = new List<Item>();

    public virtual ICollection<Modifier> Modifiers { get; } = new List<Modifier>();
}
