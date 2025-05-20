namespace DataAccessLayer.ViewModels;

public class DashboardViewModel
{
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? TimeInterval { get; set; }
    public DateTime? StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public DashboardData? DashboardData { get; set; }
 
}
public class DashboardData 
{
    public int TotalSales {get; set;}
    public int TotalOrder {get; set;}
    public int AvgOrderValue {get; set;}
    public double? AvgWaitingTime {get; set;}
    public int WaitingListCount {get; set;}
    public int TotalCustomer {get; set;}
    public List<TopLeastSellingItem>? TopSellingItems {get; set;}
    public List<TopLeastSellingItem>? LeastSellingItems { get; set; }
    public List<RevenueChartData>? RevenueByDate { get; set; }
    public List<CustomerChartData>? CustomerByDate { get; set; } // <-- Add this line

}

public class TopLeastSellingItem
{
    public int ItemId {get; set;}
    public string? ItemName {get; set;}
    public string? ImageUrl {get; set;}
    public int OrderCount {get; set;}
}
public class RevenueChartData
{
    public string Date { get; set; }
    public decimal TotalRevenue { get; set; }
}
public class CustomerChartData
{
    public string Date { get; set; }
    public int TotalCustomers { get; set; }
}