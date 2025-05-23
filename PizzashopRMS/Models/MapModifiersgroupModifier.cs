using System;
using System.Collections.Generic;

namespace PizzashopRMS.Models;

public partial class MapModifiersgroupModifier
{
    public int Mergrid { get; set; }

    public int Modifiersgroupid { get; set; }

    public int Modifiersid { get; set; }

    public virtual Modifier Modifiers { get; set; } = null!;

    public virtual Modifiersgroup Modifiersgroup { get; set; } = null!;
}
