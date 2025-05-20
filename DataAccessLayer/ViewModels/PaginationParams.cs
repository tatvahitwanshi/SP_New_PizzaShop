using Microsoft.Net.Http.Headers;
using DataAccessLayer.ViewModels;

namespace DataAccessLayer.ViewModels;

public class PaginationParams
{
    private int _PageSize = 5;
    private int _PageNumber = 1;
    private string _sortBy = "name";
    private string _sortOrder = "asc";
    private string _SearchKey = "";
    private int _Count = 0;
    
    public int PageSize{
        get=> _PageSize;
        set=> _PageSize = value > 0 ? value : 5;
    }
    public int PageNumber{
        get=> _PageNumber;
        set=> _PageNumber = value > 0 ? value : 1;
    }
    public string sortBy{
        get=> _sortBy;
        set=> _sortBy = !string.IsNullOrEmpty(value) ? value :  "name";
    }
    public string sortOrder{
        get=> _sortOrder;
        set=> _sortOrder = !string.IsNullOrEmpty(value) ? value : "asc";
    }
    public string SearchKey{
        get=> _SearchKey;
        set=> _SearchKey = !string.IsNullOrEmpty(value) ? value : "";
    }
    public int Count{
        get=> _Count;
        set=> _Count = value >= 0 ? value : 0;
    }

    public List<UserListViewModel> UserList { get; set; } 
}
public class OrdersPaginationParams
{
    private int _pageSize = 5;
    private int _pageNumber = 1;
    private string _sortColumn = "OrderNo";
    private string _sortDirection = "desc";
    private string _searchKey = "";
    private int _orderStatusId = 0;
    private string _lastDays = "All Time";
    private string _startDate = "";
    private string _endDate = "";
    public DateTime? startDateTime {get; set;} = null;
    public DateTime? endDateTime {get; set;} = null;


    public int Pagesize 
    { 
        get => _pageSize; 
        set => _pageSize = value > 0 ? value : 5; 
    }

    public int Pagenumber 
    { 
        get => _pageNumber; 
        set => _pageNumber = value > 0 ? value : 1; 
    }

    public string sortCol 
    { 
        get => _sortColumn; 
        set => _sortColumn = !string.IsNullOrEmpty(value) ? value : "OrderNo"; 
    }

    public string sortDr 
    { 
        get => _sortDirection; 
        set => _sortDirection = !string.IsNullOrEmpty(value) ? value : "asc"; 
    }

    public string Searchkey 
    { 
        get => _searchKey; 
        set => _searchKey = !string.IsNullOrEmpty(value) ? value : "";  // Prevents null values
    }

    public int OrderStatusId
    {
        get => _orderStatusId;
        set => _orderStatusId = value >= 0 ? value : 0;
    }

    public string lastDays 
    { 
        get => _lastDays; 
        set => _lastDays = !string.IsNullOrEmpty(value) ? value : "All Time";  // Prevents null values
    }

    public string startDate 
    { 
        get => _startDate; 
        set => _startDate = !string.IsNullOrEmpty(value) ? value : "";  // Prevents null values
    }

    public string endDate 
    { 
        get => _endDate; 
        set => _endDate = !string.IsNullOrEmpty(value) ? value : "";  // Prevents null values
    }
}
public class CustomersPaginationParams
{
    private int _pageSize = 5;
    private int _pageNumber = 1;
    private string _sortColumn = "Name";
    private string _sortDirection = "asc";
    private string _searchKey = "";
    private string _lastDays = "All Time";
    private string _startDate = "";
    private string _endDate = "";

    public int Pagesize 
    { 
        get => _pageSize; 
        set => _pageSize = value > 0 ? value : 5; 
    }

    public int Pagenumber 
    { 
        get => _pageNumber; 
        set => _pageNumber = value > 0 ? value : 1; 
    }

    public string SortCol 
    { 
        get => _sortColumn; 
        set => _sortColumn = !string.IsNullOrEmpty(value) ? value : "Name"; 
    }

    public string SortDr 
    { 
        get => _sortDirection; 
        set => _sortDirection = !string.IsNullOrEmpty(value) ? value : "asc"; 
    }

    public string Searchkey 
    { 
        get => _searchKey; 
        set => _searchKey = !string.IsNullOrEmpty(value) ? value : "";  // Prevents null values
    }

    public string LastDays 
    { 
        get => _lastDays; 
        set => _lastDays = !string.IsNullOrEmpty(value) ? value : "All Time";  // Prevents null values
    }

    public string StartDate 
    { 
        get => _startDate; 
        set => _startDate = !string.IsNullOrEmpty(value) ? value : "";  // Prevents null values
    }

    public string EndDate 
    { 
        get => _endDate; 
        set => _endDate = !string.IsNullOrEmpty(value) ? value : "";  // Prevents null values
    }
}
