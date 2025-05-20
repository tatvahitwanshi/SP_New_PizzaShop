using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Dish
{
    public int Dishid { get; set; }

    public int Orderid { get; set; }

    public int Itemid { get; set; }

    public string? Itemname { get; set; }

    public short Quantity { get; set; }

    public short Price { get; set; }

    public short? Pendingquantity { get; set; }

    public short? Inprogressquantity { get; set; }

    public short? Readyquantity { get; set; }

    public string? Iteminstruction { get; set; }

    public short? Inservedquantity { get; set; }

    public virtual ICollection<Dishmodifier> Dishmodifiers { get; } = new List<Dishmodifier>();

    public virtual Item Item { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
