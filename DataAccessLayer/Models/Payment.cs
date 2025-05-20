using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Payment
{
    public int Paymentid { get; set; }

    public string Paymentmode { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; } = new List<Order>();
}
