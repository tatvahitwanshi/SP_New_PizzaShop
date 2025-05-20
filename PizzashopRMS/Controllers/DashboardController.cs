using BusinessLayer.Interface;
using DataAccessLayer.Constants;
using DataAccessLayer.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PizzashopRMS.Helpers;

namespace PizzashopRMS.Controllers;

// dashboard view and applies authorization for admin users.
[CustomAuthorise(new string[] { "admin" ,"account manager"})]
public class DashboardController : BaseController
{
    private readonly IDashboard _dashboard;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboard dashboard ,ILogger<DashboardController> logger) : base()
    {
        _dashboard= dashboard;
        _logger=logger;
    }
    
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> DashboardView(string timeinterval = DashboardConst.CURRENT_MONTH, string startDate = "", string endDate = "")
    {
        DashboardViewModel model = await _dashboard.GetDashboardData(timeinterval, startDate, endDate);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> LoadDashboardDataPartial(string timeinterval = DashboardConst.CURRENT_MONTH, string startDate = "", string endDate = "")
    {
        try
        {
            DashboardViewModel model = await _dashboard.GetDashboardData(timeinterval, startDate, endDate);
            var revenueChartData = model.DashboardData.RevenueByDate; 
            var customerChartDate= model.DashboardData.CustomerByDate;

            return PartialView("_PartialDashboard", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while LoadDashboardDataPartial.");
            Console.WriteLine($"Error in LoadDashboardData: {ex.Message}");
            return StatusCode(500, "An error occurred while loading the dashboard data.");
        }
    }
    

}
