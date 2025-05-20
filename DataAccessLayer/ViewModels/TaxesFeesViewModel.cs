using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.ViewModels;

public class TaxesFeesViewModel
{
    public List<TaxesView> TaxesViewModal {get; set;} = new List<TaxesView>();
    public AddEditTaxes AddEditTaxe {get; set;}

}

public class TaxesView
{
    public int TaxesId { get; set; }

    public string Taxname { get; set; } = null!;

    public string? Taxtype { get; set; }

    public bool? Isenabled { get; set; }

    public bool? Isdefault { get; set; }

    public double? Taxvalue { get; set; }

    public bool? Isdeleted { get; set; }

}

public class AddEditTaxes
{
    public int TaxesId { get; set; }

    [Required(ErrorMessage = "Tax name is required.")]
    public string Taxname { get; set; } = null!;

    [Required(ErrorMessage = "Tax type is required.")]
    public string? Taxtype { get; set; }

    public bool Isenabled { get; set; }

    public bool Isdefault { get; set; }

    [Required(ErrorMessage = "Tax value is required.")]
    [Range(0, double.MaxValue, ErrorMessage = "Tax value must be a positive number.")]
    public double? Taxvalue { get; set; }

    public bool? Isdeleted { get; set; }
    public string? CreatedBy { get; set; }
    public string? EditedBy { get; set; }
    public DateTime? EditDate { get; set; }
}