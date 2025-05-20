using DataAccessLayer.ViewModels;

namespace BusinessLayer.Interface;

public interface IKot
{
    public Task<List<CategoryList>> GetCategoryLists();
    public Task<string> GetCategoryName(int id);
    public List<OrderCards> GetOrderCards(int CurrentCategory, string orderStatus);

    public Task UpdateChangeQuantity(OrderCards ordercard, string OrderStatus);
    public Task<List<PendingOrders>> GetPendingOrders();
    public Task SetToInProgress(List<int> model);

    public bool IsServed(int orderId);
    public Task<(bool, string)> MarkOrderServed(int orderId);


}
