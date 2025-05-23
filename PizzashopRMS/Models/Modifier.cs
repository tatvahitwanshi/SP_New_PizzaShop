using System;
using System.Collections.Generic;

namespace PizzashopRMS.Models;

public partial class Modifier
{
    public int Modifiersid { get; set; }

    public string Modifiersname { get; set; } = null!;

    public int? Modifiersunit { get; set; }

    public int? Modifiersrate { get; set; }

    public int? Modifiersquantity { get; set; }

    public string? Modifiersdescription { get; set; }

    public bool? Isdeleted { get; set; }

    public string? CreatedBy { get; set; }

    public string? EditedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? EditDate { get; set; }

    public virtual ICollection<Dishmodifier> Dishmodifiers { get; } = new List<Dishmodifier>();

    public virtual ICollection<MapModifiersgroupModifier> MapModifiersgroupModifiers { get; } = new List<MapModifiersgroupModifier>();

    public virtual ItemsUnit? ModifiersunitNavigation { get; set; }
}
