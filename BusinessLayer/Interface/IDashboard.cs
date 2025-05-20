using DataAccessLayer.ViewModels;

namespace BusinessLayer.Interface;

public interface IDashboard
{
    public Task<DashboardViewModel> GetDashboardData(string TimeInterval, string StartDate, string EndDate);

}
