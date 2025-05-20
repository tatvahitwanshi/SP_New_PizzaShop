using BusinessLayer.Interface;
using DataAccessLayer.Constants;
using DataAccessLayer.ViewModels;
using Microsoft.AspNetCore.Mvc;
using PizzashopRMS.Helpers;

namespace PizzashopRMS.Controllers;

[CustomAuthorise(new string[] { "admin" ,"account manager"}, PermissionConst.CanView, PermissionConst.CUSTOMERS)]

public class CustomersController : BaseController
{
    private readonly ICustomers _customer;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(ICustomers custServices,ILogger<CustomersController> logger) : base(PermissionConst.CUSTOMERS)
    {
        _customer = custServices;
        _logger=logger;

    }
    [HttpGet]
    public async Task<IActionResult> CustomersView(CustomersPaginationParams paramsModel)
    {
        try
        {
            CustomersViewModel model = new CustomersViewModel
            {
                CustomerPages = await _customer.GetCustomers(paramsModel)
            };
            return View(model);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error while CustomersView.");
            TempData["error"] = $"Failed to load customer data. Error: {ex.Message}";
            return View(new CustomersViewModel());
        }
    }
    [HttpGet]
    public async Task<IActionResult> CustomerListTable(CustomersPaginationParams paramsModel)
    {
        try
        {
            CustomersViewModel model = new CustomersViewModel
            {
                CustomerPages = await _customer.GetCustomers(paramsModel)
            };
            return PartialView("_PartialCustomersTable", model);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error while CustomerListTable.");
            return BadRequest(ex);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(string LastDays = "All Time", string Searchkey = "", string StartDate = "", string EndDate = "")
    {
        try
        {
            byte[] fileBytes = await _customer.ExportToExcelFile(LastDays, Searchkey, StartDate, EndDate);
            if (fileBytes == null || fileBytes.Length == 0) 
                return BadRequest("No records found");        
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Customers.xlsx");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error while ExportToExcel.");
            TempData["error"] = $"Export to Excel failed. Error: {ex.Message}";
            return RedirectToAction("CustomersView");
        }
    }


    [HttpGet]
    public IActionResult CustomersHistory(int id)
    {
        try
        {
            CustomerDetailsList model = _customer.GetCustomerDetails(id);
            return PartialView("_CustomersHistory", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while CustomersHistory.");
            return BadRequest(ex);
        }
    }

}
