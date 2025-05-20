using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Orderapp
{
    public int Orderappid { get; set; }

    public int Orderid { get; set; }

    public int Itemid { get; set; }

    public int? Quantity { get; set; }

    public string? Comment { get; set; }

    public string? ItemComment { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual ICollection<KotTable> KotTables { get; } = new List<KotTable>();

    public virtual Order Order { get; set; } = null!;
}
