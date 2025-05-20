using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;

namespace BusinessLayer.Interface;

public interface IOrders
{
    public OrderPage GetOrders(OrdersPaginationParams paramsModel);
    List<Orderstatus> GetOrderStatus();
    public Task<byte[]> ExportToExcel(int orderStatusId = 0, string lastDays = "All Time", string searchKey = "");
    public Task<OrderDetailsViewModel> getOrderDetails(int orderId = 1);
    public Task<byte[]> ExportToPdf(int orderId = 1);


}
