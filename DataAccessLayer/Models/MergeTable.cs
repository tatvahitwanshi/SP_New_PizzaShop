using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class MergeTable
{
    public int Mergrid { get; set; }

    public int Tablesid { get; set; }

    public int Orderid { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Table Tables { get; set; } = null!;
}
