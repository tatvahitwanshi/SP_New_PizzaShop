using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;

namespace BusinessLayer.Interface;

public interface IDashboard
{
    public Task<DashboardViewModel> GetDashboardData(string TimeInterval, string StartDate, string EndDate);
    Task<int> GetWaitingListCount(DateTime start, DateTime end);



}
