using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Section
{
    public int Sectionid { get; set; }

    public string Sectionname { get; set; } = null!;

    public string? Sectiondescription { get; set; }

    public bool? Isdeleted { get; set; }

    public string? CreatedBy { get; set; }

    public string? EditedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? EditDate { get; set; }

    public virtual ICollection<Table> Tables { get; } = new List<Table>();

    public virtual ICollection<WaitingTable> WaitingTables { get; } = new List<WaitingTable>();
}
