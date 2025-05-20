using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Dishmodifier
{
    public int Dishmodifiersid { get; set; }

    public int Dishid { get; set; }

    public int Modifierid { get; set; }

    public string? Modifiername { get; set; }

    public short Quantity { get; set; }

    public short Price { get; set; }

    public virtual Dish Dish { get; set; } = null!;

    public virtual Modifier Modifier { get; set; } = null!;
}
