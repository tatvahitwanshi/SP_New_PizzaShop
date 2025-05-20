using DataAccessLayer.Models;

namespace DataAccessLayer.ViewModels;

public class OrdersViewModel
{
    // public List<OrderList> OrderLists { get; set; } = new List<OrderList>();

    public List<Orderstatus> OrderStatus { get; set; } = new List<Orderstatus>();

    public OrderPage OrderPage { get; set; }
}

public class OrderPage
{
    public int? OrderStatusId { get; set; }
    public int? Pagenumber {get; set;}
    public int? Pagesize {get; set;}
    public string? Searchkey {get; set;}
    public string? sortDr {get; set;}
    public string? sortCol {get; set;}
    public string? lastDays {get; set;}
    public DateTime? startDate {get; set;}
    public DateTime? endDate {get; set;}  
    public int? Count{get; set;}
  
    public List<OrderList> OrderTableLists { get; set; } = new List<OrderList>();

}

public class OrderList
{
    public int Orderid { get; set; }

    public string? CustomerName { get; set; }

    public string? Status { get; set; }

    public string? PaymentMode { get; set; }

    public short? Rating { get; set; }

    public decimal? TotalAmount { get; set; }

    public DateTime? CreatedDate { get; set; }


}

