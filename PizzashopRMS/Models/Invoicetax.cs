using System;
using System.Collections.Generic;

namespace PizzashopRMS.Models;

public partial class Invoicetax
{
    public int Invoicetaxid { get; set; }

    public int? Invoiceid { get; set; }

    public int Taxid { get; set; }

    public string? Taxname { get; set; }

    public decimal Taxvalue { get; set; }

    public string Taxvaluetype { get; set; } = null!;

    public virtual Invoice? Invoice { get; set; }

    public virtual Taxis Tax { get; set; } = null!;
}
