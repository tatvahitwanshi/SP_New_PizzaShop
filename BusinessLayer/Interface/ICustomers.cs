using DataAccessLayer.ViewModels;

namespace BusinessLayer.Interface;

public interface ICustomers
{
    public Task<CustomerPage> GetCustomers(CustomersPaginationParams paramsModel);

    public Task<byte[]> ExportToExcelFile(string LastDays = "All Time",string Searchkey = "", string StartDate = "", string EndDate = "");
     public CustomerDetailsList GetCustomerDetails(int cutomerId);

}
