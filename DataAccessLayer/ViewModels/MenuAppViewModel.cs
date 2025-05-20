using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.ViewModels;

public class MenuAppViewModel
{
    public int? CurrCategoryID {get; set;}
    public List<CategoryList>? CategoryList {get; set;}
    public List<SingleItemForOrderApp>? ItemList {get; set;}
    public int TableId {get; set;}
    public TokenOrOrderDetails? TokenOrderDetails {get; set;}
      // taxes 
    public List<TaxesView>? TaxList {get; set;}


}
public class TokenOrOrderDetails
{
    public string? TokenORorder {get; set;}
    public int? Id {get; set;}
    public int? TokenId {get; set;}
    public int? OrderId {get; set;}
    public int? CustomerId {get; set;}
    public int? NumberOfPerson {get; set;}
    public string? Instruction {get; set;}
    public int? MaxTableCapacity {get; set;}
    public List<OrderedItem>? CurrOrder {get; set;}

    public List<int>? CurrTaxes {get; set;}

    public CustomerDetails? CustomerDetail {get; set;}
    public TableDetails? TableDetail {get; set;}
    public string? By {get; set;}

}


public class SingleItemForOrderApp
{
    public int ItemId {get; set;}

    public string ItemName {get; set;}

    public string ItemType {get; set;}

    public int Rate { get; set; }

    public int Quantity { get; set; }

    public bool Isavailable { get; set; }

    public string? ImageUrl {get; set;}

    public bool? isFavourite {get; set;}

    public bool? isModifier {get; set;}
}

public class ModifierDetailsForItem
{
    public int? ItemId {get; set;}
    public string? ItemName {get; set;}
    public string? ItemType {get; set;}
    public int? Price {get; set;}
    public List<ModifierGroupDetailsOrderApp>? ModGroupList {get; set;}
}

public class ModifierGroupDetailsOrderApp
{
    public int? ModGroupId {get; set;}
    public string? ModGroupName {get; set;}
    public int? Min {get; set;}
    public int? Max {get; set;}
    public List<ModifierItems>? ModifierList{get; set;}
}

public class ModifierItems
{
    public int? ModifierId {get; set;}
    public string? ModifierName {get; set;}
    public int? Price {get; set;}
}

public class OrderedItem
{
    public int? Id {get; set;}
    public int? ItemId {get; set;}
    public string? ItemName {get; set;}
    public string? ItemType {get; set;}
    public int? Price {get; set;}
    public int? Quantity {get; set;}
    public string? Instruction {get; set;}
    public decimal? ItemPriceTotal {get; set;}
    public decimal? ModifierPriceTotal {get; set;}
    public List<OrderedModifier>? ModifierList{get; set;}
    public bool? IsEdit {get; set;}
    public bool? IsServed {get; set;}
    public short? ReadyQuantity {get; set;}
    public short? ServedQuantity {get; set;}

} 

public class OrderedModifier
{
    public int? ModifierGroup {get; set;}
    public int? ModifierId {get; set;}
    public string? ModifierName {get; set;}
    public string? ModifierGroupName {get; set;}
    public int? Price {get; set;}
    public int? Quantity {get; set;}
}

public class CustomerDetailsOrderApp
{
    public string? TokenOrOrder {get; set;}
    public int? Id { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string? CustomerEmail { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name can't be longer than 100 characters")]
    public string? CustomerName { get; set; }

    [Required(ErrorMessage = "Mobile No. is required")]
    [Phone(ErrorMessage = "Invalid Mobile Number")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "Mobile No. must be between 10 digits")]
    public string? MobileNo { get; set; }

    [Required(ErrorMessage = "Number of persons is required")]
    [Range(1, 20, ErrorMessage = "Number of persons must be between 1 and 20")]
    
    public short? NumberOfPersons { get; set; }
    public int? By {get; set;}
}



public class CompleteOrderApp
{
    public int OrderId {get; set;}
    public string? Comments {get; set;}
    public int Food {get; set;}
    public int Service {get; set;}
    public int Ambience {get; set;}
    public List<int> TaxList {get; set;} = new List<int>();
    public string? By {get; set;}
    public string? PaymentMethod {get; set;}


}