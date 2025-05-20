using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.ViewModels;

public class WaitingListViewModel
{
    public int CurrentId { get; set; }
    public List<SectionDetails>? SectionList {get; set;}
    public List<TokenDetail>? TokenList { get; set; }
    public AssignToken? AssignToken { get; set; }
}

public class TableLists
{
    public int? SectionId { get; set; }
    public List<TableSingle>? TableList { get; set; }
}

public class SectionDetails
{
    public int SectionId { get; set; }
    public string? SectionName { get; set; }
    public int? TotalToken {get; set;}
}

public class TokenDetail
{
    public int TokenId { get; set; }
    public string? TokenNo { get; set; }
    public string? CustomerName { get; set; }
    public string? PhoneNo { get; set; }
    public string? Email { get; set; }
    public int? NoOfPerson { get; set; }
    public DateTime? CreatedDate {get; set;}
}


public class AssignToken
{
    public int TokenId { get; set; }

    [Required(ErrorMessage = "Section is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Section is required")]
    public int? SectionId { get; set; }

    [Required(ErrorMessage = "Table is required")]
    public int? TableId { get; set; }

    public string? By{get; set;}
}

public class TableSingle
{
    public int? TableId { get; set; }
    public string? TableName { get; set; }
}

public class WaitingToken
{
    public int? TokenId { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string? CustomerEmail { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name can't be longer than 100 characters")]
    public string? CustomerName { get; set; }

    [Required(ErrorMessage = "Mobile No. is required")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "Mobile No. must be between 10 digits")]
    public string? MobileNo { get; set; }

    [Required(ErrorMessage = "Number of persons is required")]
    [Range(1, 20, ErrorMessage = "Number of persons must be between 1 and 20")]
    public short? NumberOfPersons { get; set; }

    [Required(ErrorMessage = "Section is required")]
    public int? SectionId { get; set; }

    public List<SectionDetails>? SectionList { get; set; }
    public string? By { get; set; }
}
