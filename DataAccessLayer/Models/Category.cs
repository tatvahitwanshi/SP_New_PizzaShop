using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Category
{
    public int Categoryid { get; set; }

    public string Categoryname { get; set; } = null!;

    public string? Categorydescription { get; set; }

    public bool? Isdeleted { get; set; }

    public string? CreatedBy { get; set; }

    public string? EditedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? EditDate { get; set; }

    public virtual ICollection<Item> Items { get; } = new List<Item>();
}
