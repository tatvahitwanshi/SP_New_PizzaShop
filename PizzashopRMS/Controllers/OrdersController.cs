using BusinessLayer.Interface;
using DataAccessLayer.Constants;
using DataAccessLayer.ViewModels;
using Microsoft.AspNetCore.Mvc;
using PizzashopRMS.Helpers;

namespace PizzashopRMS.Controllers;

[CustomAuthorise(new string[] { "admin","account manager" }, PermissionConst.CanView, PermissionConst.ORDERS)]
public class OrdersController : BaseController
{
    private readonly IOrders _orders;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrders orders,ILogger<OrdersController> logger) : base(PermissionConst.ORDERS)
    {
        _orders = orders;
        _logger=logger;
    }

    public IActionResult OrdersView()
    {
        try
        {
            OrdersViewModel model = new OrdersViewModel
            {
                OrderStatus = _orders.GetOrderStatus(),
                OrderPage = _orders.GetOrders(new OrdersPaginationParams())
            };
            return View(model);
        }
        catch (Exception ex)
        {
            TempData["error"] = "Something went wrong while loading orders. " + ex.Message;
            return RedirectToAction("DashboardView", "Dashboard");        
        }

    }

    [HttpGet]
    public IActionResult OrderListTable(OrdersPaginationParams paramsModel)
    {
        try
        {
            paramsModel.startDateTime = string.IsNullOrEmpty(paramsModel.startDate) ? null : DateTime.Parse(paramsModel.startDate);
            paramsModel.endDateTime = string.IsNullOrEmpty(paramsModel.endDate) ? null : DateTime.Parse(paramsModel.endDate);

            OrdersViewModel model = new OrdersViewModel
            {
                OrderStatus = _orders.GetOrderStatus(),
                OrderPage = _orders.GetOrders(paramsModel)
            };
            return PartialView("_PartialOrderList", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while Order List Table.");
            return BadRequest();
        }
    }


    [HttpGet]
    public async Task<IActionResult> ExportToExcelFile(int OrderStatusId = 0, string lastDays = "All Time", string Searchkey = "")
    {
        try
        {
            byte[] fileBytes = await _orders.ExportToExcel(OrderStatusId, lastDays, Searchkey);
            if (fileBytes == null || fileBytes.Length == 0) 
                return BadRequest("No records found");  
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Orders.xlsx");
        }
        catch (Exception ex)
        {
            TempData["error"] = "Export to Excel failed. " + ex.Message;
            return BadRequest("An error occurred during export.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> OrderDetails(int orderId = 1)
    {
        try
        {
            OrderDetailsViewModel model = await _orders.getOrderDetails(orderId);
            return View(model);
        }
        catch (Exception ex)
        {
            TempData["error"] = "Failed to load order details. " + ex.Message;
            return View(new OrderDetailsViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportToPdf(int orderId = 1)
    {
        try
        {
            byte[] fileBytes = await _orders.ExportToPdf(orderId);
            return File(fileBytes, "application/pdf", "Order.pdf");
        }
        catch (Exception ex)
        {
            TempData["error"] = "Export to PDF failed. " + ex.Message;
            return BadRequest("PDF generation error.");
        }
    }
}


