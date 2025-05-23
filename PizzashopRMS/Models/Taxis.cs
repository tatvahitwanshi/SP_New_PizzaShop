using System;
using System.Collections.Generic;

namespace PizzashopRMS.Models;

public partial class Taxis
{
    public int Taxesid { get; set; }

    public string Taxname { get; set; } = null!;

    public string? Taxtype { get; set; }

    public bool? Isenabled { get; set; }

    public bool? Isdefault { get; set; }

    public double? Taxvalue { get; set; }

    public bool? Isdeleted { get; set; }

    public string? CreatedBy { get; set; }

    public string? EditedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? EditDate { get; set; }

    public virtual ICollection<Invoicetax> Invoicetaxes { get; } = new List<Invoicetax>();

    public virtual ICollection<Item> Items { get; } = new List<Item>();
}
