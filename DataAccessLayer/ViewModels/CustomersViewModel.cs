namespace DataAccessLayer.ViewModels;

public class CustomersViewModel
{
    public CustomerPage? CustomerPages {get; set;}
}
public class CustomerPage
{
    public int? Pagenumber {get; set;}
    public int? Pagesize {get; set;}
    public string? Searchkey {get; set;}
    public string? LastDays {get; set;}
    public int? Count{get; set;}
    public string? SortDr{get; set;}
    public string? SortCol{get; set;}
    public DateTime? StartDate {get; set;}
    public DateTime? EndDate {get; set;}
    public List<CustomerList>? CustomerLists {get; set;}
}

public class CustomerList
{
    public int? CustomerId {get; set;}
    public string? CustomerName {get; set;}
    public string? CustomerEmail {get; set;}
    public string? PhoneNumber {get; set;}
    public int TotalOrder {get; set;}
    public DateTime? LastOrder { get; set; }

}

public class CustomerDetailsList
{
    public int? CustomerId {get; set;}
    public string? CustomerName {get; set;}
    public string? PhoneNumber {get; set;}
    public decimal? AvgBill { get; set; }
    public decimal? MaxBill { get; set; }
    public DateTime? ComingSince {get; set;}
    public int Visits {get; set;}
    public List<CustomerOrders>? Orders {get; set;}
}

public class CustomerOrders
{
    public DateOnly? OrderDate {get; set;}
    public string? OrderType {get; set;}
    public string? PaymentType {get; set;}
    public int TotalItem {get; set;}
    public decimal? TotalAmount {get; set;}
}


