using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class KotTable
{
    public int Kotid { get; set; }

    public int Orderappid { get; set; }

    public bool ItemStatus { get; set; }

    public bool OrderStatus { get; set; }

    public string? CreatedBy { get; set; }

    public string? EditedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? EditDate { get; set; }

    public virtual Orderapp Orderapp { get; set; } = null!;
}
