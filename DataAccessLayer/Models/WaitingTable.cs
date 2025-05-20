using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class WaitingTable
{
    public int Waitingid { get; set; }

    public int Customerid { get; set; }

    public bool? Isdeleted { get; set; }

    public string? CreatedBy { get; set; }

    public string? EditedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? EditDate { get; set; }

    public int Sectionid { get; set; }

    public int Tokennumber { get; set; }

    public DateTime Tokendate { get; set; }

    public DateTime? Assigntime { get; set; }

    public short Totalperson { get; set; }

    public bool? Isassigned { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<MapTableToken> MapTableTokens { get; } = new List<MapTableToken>();

    public virtual Section Section { get; set; } = null!;
}
