using DataAccessLayer.ViewModels;

namespace BusinessLayer.Interface;

public interface ITablesApp
{
    public Task<List<SectionOrderView>> getTableOrders();
    public Task<(bool, string)> assignToken(WaitingToken model, List<int>? tableIdsForAssign);
    public int getTokenId(int tableId);

}
