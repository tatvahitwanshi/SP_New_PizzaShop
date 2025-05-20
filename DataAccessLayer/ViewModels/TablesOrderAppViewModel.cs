namespace DataAccessLayer.ViewModels;

public class TablesOrderAppViewModel
{
    public List<SectionOrderView>? sectionList {get; set;}

}
public class SectionOrderView
{
    public int SectionId {get; set;}
    public string? SectionName {get; set;}
    public int? AvailableCount {get; set;}
    public int? AssignedCount {get; set;}
    public int? RunningCount {get; set;}
    public List<TableOrderView>? TableList {get; set;}
}

public class TableOrderView
{
    public int TableId {get; set;}
    public string? TableName {get; set;}
    public int? Capacity {get; set;}
    public bool? isOccupied {get; set;}
    public bool? isRunning {get; set;}
    public int? OrderTokenId {get; set;}
    public decimal? OrderAmount {get; set;}
    public string? Status {get; set;}
    public DateTime? Time {get; set;}

}