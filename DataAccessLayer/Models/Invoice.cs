using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Invoice
{
    public int Invoiceid { get; set; }

    public string? Invoicenumber { get; set; }

    public int Orderid { get; set; }

    public DateTime? Paidon { get; set; }

    public virtual ICollection<Invoicetax> Invoicetaxes { get; } = new List<Invoicetax>();

    public virtual Order Order { get; set; } = null!;
}
