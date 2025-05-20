using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Item
{
    public int Itemid { get; set; }

    public string Itemname { get; set; } = null!;

    public int? Rate { get; set; }

    public string Itemtype { get; set; } = null!;

    public int? Quantity { get; set; }

    public bool? Isavailable { get; set; }

    public bool? Isdeleted { get; set; }

    public string? Itemdescription { get; set; }

    public string? Itemimage { get; set; }

    public int Categoryid { get; set; }

    public int? Taxesid { get; set; }

    public int Unitid { get; set; }

    public string? CreatedBy { get; set; }

    public string? EditedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? EditDate { get; set; }

    public bool? Defaulttax { get; set; }

    public decimal? Taxpercentage { get; set; }

    public string? Shortcode { get; set; }

    public bool? Isfavourite { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Dish> Dishes { get; } = new List<Dish>();

    public virtual ICollection<MapItemsModifiersgroup> MapItemsModifiersgroups { get; } = new List<MapItemsModifiersgroup>();

    public virtual ICollection<Orderapp> Orderapps { get; } = new List<Orderapp>();

    public virtual Taxis? Taxes { get; set; }

    public virtual ItemsUnit Unit { get; set; } = null!;
}
