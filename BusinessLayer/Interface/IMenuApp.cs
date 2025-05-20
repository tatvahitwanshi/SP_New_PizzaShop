using DataAccessLayer.ViewModels;

namespace BusinessLayer.Interface;

public interface IMenuApp
{
    public Task<List<CategoryList>> GetCategoryLists();
    public  Task<int> checkTableValue(int tableId);
    public Task<List<TaxesView>> GetTaxList();

    public Task<List<SingleItemForOrderApp>> getItemList(int categoryId, string SearchKey);

    public Task<bool> MarkFavouriteAsync(int itemId, bool isFavourite);
    public Task<ModifierDetailsForItem> getModifierItemDetails(int itemId);
    public Task<TokenOrOrderDetails?> getTokenOrderDetails(int tableId);
    public OrderedItem calculateItemValuesPrice(OrderedItem model);
    public Task<string> SavePlaceOrder(TokenOrOrderDetails model);

    public Task<(bool, string, int)> UpdateCustomerDetails(CustomerDetailsOrderApp model);
    public bool IsOrderCompletedToServed(int orderId);
    public  Task<bool> CompleteOrder(CompleteOrderApp model);
    public Task<bool> CancelOrderApp(int orderId, string by);



}
