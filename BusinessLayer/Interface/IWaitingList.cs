using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;

namespace BusinessLayer.Interface;

public interface IWaitingList
{
    public Task<List<SectionDetails>> getSectionList();
    public Task<List<TokenDetail>> getTokenList(int sectionId);
    public Task<List<TableSingle>> getTableList(int sectionId, int capacity);
    public Task<WaitingToken> getToken(int id);
    public Task generateToken(WaitingToken token);
    public Task assignToken(AssignToken assignToken);
    public Task deleteToken(int id);
    public Task<Customer?> getCustomerFromEmail(string email);

}
