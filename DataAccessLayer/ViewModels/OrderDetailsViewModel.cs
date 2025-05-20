namespace DataAccessLayer.ViewModels;

public class OrderDetailsViewModel
{
    public int OrderId {get; set;}
    public DateTime? CreatedDate { get; set; }
    public DateTime CompletedTime {get; set;}
    public DateTime? PaidOn {get; set;}
    public int InvoiceId {get; set;}
    public string? OrderStatus { get; set; }
    public string? PaymentStatus { get; set; }
    public short Personcount { get; set; }
    public CustomerDetails? Customer{get;set;}
    public TableDetails? Tables { get; set; }
    public List<DishDetails>? Dishes { get; set; }
    public List<TaxDetails>? Taxes { get; set; }
    public int SubTotal { get; set; }
    public decimal Total { get; set; }
    public string? InvoiceNumber{get; set;}
}
public class CustomerDetails
{ 
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
public class TableDetails
{
    public string? SectionName { get; set; }
    public List<string?>? TableList { get; set; }
}
public class DishDetails
{
    public string? Itemname { get; set; }
    public short Quantity { get; set; }
    public short Price { get; set; }
    public int Total { get; set; }
    public List<ModifierDetails>? modifiers { get; set; }
}
public class ModifierDetails
{
    public string? Modifiername { get; set; }
    public short Quantity { get; set; }
    public short Price { get; set; }
    public int Total { get; set; }
}
public class TaxDetails
{
    public string? Taxname { get; set; }
    public decimal Taxvalue { get; set; }
    public string? Taxvaluetype { get; set; }
    public decimal AppliedTax { get; set; }
}
