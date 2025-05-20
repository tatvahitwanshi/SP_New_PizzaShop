using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.ViewModels;

public class SectionTablesViewModal
{
    public List<SectionViewModal> SectionViewModals { get; set; } = new List<SectionViewModal>();

    public AddEditSectionViewModal AddEditSection {get; set;}

    public Pagination<TablesView> TablesViews { get; set; } = new Pagination<TablesView>();

    public AddEditTablesView AddEditTables {get; set;}

}
public class SectionViewModal
{
    public int SectionId { get; set; }
    public string? Sectionname { get; set; }
    public string? Sectiondescription { get; set; }
    public bool? Isdeleted { get; set; }

}

public class AddEditSectionViewModal
{
    public int SectionId { get; set; }
    
    [Required(ErrorMessage = "Section name is required.")]
    public string? Sectionname { get; set; }

    [Required(ErrorMessage = "Section description is required.")]
    public string? Sectiondescription { get; set; }

    public bool? Isdeleted { get; set; }

    public string? CreatedBy { get; set; }

    public string? EditedBy { get; set; }

    public DateTime? EditDate { get; set; }

}

public class TablesView
{
    public int TablesId { get; set; }

    public string Tablename { get; set; } = null!;

    public string? Tablecapacity { get; set; }

    public bool? Isoccupied { get; set; }

    public bool? Isdeleted { get; set; }

    public int? SectionId { get; set; }


}

public class AddEditTablesView
{
   
    public int TablesId { get; set; }

    [Required(ErrorMessage = "Table name is required.")]
    public string Tablename { get; set; } = null!;

    public string? Tablecapacity { get; set; }

    [Range(1, 20, ErrorMessage = "Capacity must be between 1 and 20.")]
    [Required]
    public bool? Isoccupied { get; set; }

    public bool? Isdeleted { get; set; }

    public int? SectionId { get; set; }

    public string? CreatedBy { get; set; }

    public string? EditedBy { get; set; }

    public DateTime? EditDate { get; set; }


}

