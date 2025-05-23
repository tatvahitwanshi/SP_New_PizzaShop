using System;
using System.Collections.Generic;

namespace PizzashopRMS.Models;

public partial class Modifiersgroup
{
    public int Modifiersgroupid { get; set; }

    public string Modifiersgroupname { get; set; } = null!;

    public string? Modifiersgroupdescription { get; set; }

    public bool? Isdeleted { get; set; }

    public string? CreatedBy { get; set; }

    public string? EditedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? EditDate { get; set; }

    public virtual ICollection<MapItemsModifiersgroup> MapItemsModifiersgroups { get; } = new List<MapItemsModifiersgroup>();

    public virtual ICollection<MapModifiersgroupModifier> MapModifiersgroupModifiers { get; } = new List<MapModifiersgroupModifier>();
}
