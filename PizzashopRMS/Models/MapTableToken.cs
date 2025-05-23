using System;
using System.Collections.Generic;

namespace PizzashopRMS.Models;

public partial class MapTableToken
{
    public int Id { get; set; }

    public int Tokenid { get; set; }

    public int Tableid { get; set; }

    public virtual Table Table { get; set; } = null!;

    public virtual WaitingTable Token { get; set; } = null!;
}
