namespace DataAccessLayer.ViewModels;

public class KotViewModel
{
    public List<CategoryList>? CategoryList {get; set;}
    public int? CurrentCategory {get; set;}
    public string? CategoryName {get; set;}
    public string? OrderStatus {get; set;}
    public List<OrderCards>? Cards {get; set;}


}
public class CategoryList
{
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
}
public class OrderCards
{
    public int? OrderId {get; set;}
    public DateTime? OrderTime {get; set;}
    public List<string>? TableNameList {get; set;}
    public string? SectionName {get; set;}
    public List<SingleItem>? ItemList {get; set;}
    public string? OrderInstructions {get; set;}
}

public class SingleItem
{
    public int? DishId {get; set;}
    public int? ItemId {get; set;}
    public string? ItemName {get; set;}
    public int? inProgressQuantity {get; set;}
    public int? pendingQuantity {get; set;}
    public int? Quantity {get; set;}
    public int? TotalQuantity {get; set;}
    public bool IsSelected {get; set;}
    public string? ItemInstructions {get; set;}
    public List<SingleModifier> ? ModifiersList {get; set;}
}

public class SingleModifier
{
    public int? ModifierId {get; set;}
    public string? ModifierName {get; set;}
}

public class PendingOrders
{
    public int OrderID {get; set;}
    public decimal? Amount {get; set;}
    public DateTime OrderTime {get; set;}
}