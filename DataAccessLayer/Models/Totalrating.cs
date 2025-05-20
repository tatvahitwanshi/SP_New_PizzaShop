using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Totalrating
{
    public int Ratingid { get; set; }

    public short Foodrating { get; set; }

    public short Servicerating { get; set; }

    public short Ambiancerating { get; set; }

    public string? Comments { get; set; }

    public int Orderid { get; set; }

    public virtual Order Order { get; set; } = null!;
}
