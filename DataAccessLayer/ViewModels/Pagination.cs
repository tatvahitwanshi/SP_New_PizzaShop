namespace DataAccessLayer.ViewModels;

public class Pagination<T>
{
    public int ParentId { get; set; }
    public int PageSize { get; set; } 
    public int PageNumber { get; set; } 
    public int Count { get; set; }
    public int MaxPage { get; set; }
    public string SearchKey { get; set; }
    public List<T> List { get; set; }
    

}
