using System.Threading.Tasks;
using BusinessLayer.Interface;
using DataAccessLayer.Constants;
using DataAccessLayer.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using PizzashopRMS.Helpers;

namespace PizzashopRMS.Controllers;
[CustomAuthorise(new string[] { "admin","account manager" })]
public class MenuAppController : BaseOrderAppController
{
    private readonly IMenuApp _menuApp;
    private readonly IOrders _orders;
    private readonly ILogger<MenuAppController> _logger;
    private readonly IHubContext<HubSignalR> _hubContext;
    private readonly IDashboard _dashboard;


    public MenuAppController(IMenuApp menuApp, IOrders orders, ILogger<MenuAppController> logger, IHubContext<HubSignalR> hubContext, IDashboard dashboard)
    {
        _menuApp = menuApp;
        _orders = orders;
        _logger = logger;
        _dashboard = dashboard;
        _hubContext = hubContext;
    }

    public async Task<IActionResult> MenuApp(int tableId = -1)
    {
        try
        {
            MenuAppViewModel model = new MenuAppViewModel
            {
                TableId = await _menuApp.checkTableValue(tableId),
                CategoryList = await _menuApp.GetCategoryLists(),
                CurrCategoryID=-1,
                TaxList = await _menuApp.GetTaxList(),
            
            };
            if(tableId != -1) model.TokenOrderDetails = await _menuApp.getTokenOrderDetails(tableId);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while MenuApp.");
            TempData["error"] = $"Failed to load Menu App. Error: {ex.Message}";
            return View(new MenuAppViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> ChangeCategory(int CategoryId, string SearchKey = "")
    {
        try
        {
            MenuAppViewModel model = new MenuAppViewModel
            {
                CurrCategoryID = CategoryId,
                ItemList = await _menuApp.getItemList(CategoryId, SearchKey)
            };

            return PartialView("_PartialMenuItems", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while ChangeCategory.");
            TempData["error"] = $"Failed to load category items. Error: {ex.Message}";
            return PartialView("_PartialMenuItems", new MenuAppViewModel());
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> MarkFavouriteItem(int itemId, bool isFavourite)
    {
        try
        {
            var success = await _menuApp.MarkFavouriteAsync(itemId, isFavourite);
            if (!success)
                return NotFound(new { success = false, message = "Item not found" });

            return Ok(new { success = true, message = "Favourite status updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while MarkFavouriteItem.");
            TempData["error"] = $"Error updating favourite item. Error: {ex.Message}";
            return Json(new { success = false });
        }
    }
    [HttpGet]
    public async Task<IActionResult> GetModifierList(int ItemId)
    {
        try
        {
            ModifierDetailsForItem model = await _menuApp.getModifierItemDetails(ItemId);
            return PartialView("_PartialModifierModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while Get Modifier List.");
            return BadRequest(ex);        
        }
    } 

    [HttpGet]
    public IActionResult AddOrderedItem(string itemValues)
    {
        try
        {
            OrderedItem model = new OrderedItem();
            if(!string.IsNullOrEmpty(itemValues))
            {
                model = JsonConvert.DeserializeObject<OrderedItem>(itemValues)!;
                model = _menuApp.calculateItemValuesPrice(model);
            }
            return PartialView("_PartialOrderItems", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while Add Ordered Item.");
            return BadRequest();
        }
    }

    public async Task<IActionResult> PlaceOrder(string placeOrder)
    {
        try
        {
            string message = "Order Placed Successfully";
            TokenOrOrderDetails model = new TokenOrOrderDetails();
            if (!string.IsNullOrEmpty(placeOrder))
            {
                model = JsonConvert.DeserializeObject<TokenOrOrderDetails>(placeOrder)!;
                if(model != null)
                {
                    model.By= getUserEmail();
                    message= await _menuApp.SavePlaceOrder(model);
                }
            }
            return Json(new { success = true, message = message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while Place Order.");
            return Json(new { success = false });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomerDetailsModal(int numberOfPerson, string tokenOrOrder, int id, string customerDetails)
    {
        try
        {
            CustomerDetails model = new CustomerDetails();
            if(!string.IsNullOrEmpty(customerDetails))
            {
                model = JsonConvert.DeserializeObject<CustomerDetails>(customerDetails)!;;
            }

            CustomerDetailsOrderApp newModel = new CustomerDetailsOrderApp
            {
                Id = id,
                TokenOrOrder = tokenOrOrder,
                CustomerName = model.Name,
                CustomerEmail = model.Email,
                MobileNo = model.Phone,
                NumberOfPersons = (short?)numberOfPerson
            };
            return PartialView("_PartialCustomersDetail", newModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while Get Customer Details Modal.");
            return BadRequest();
        }
    }

    [HttpPost]
    public async Task<IActionResult> SaveCustomerDetails(CustomerDetailsOrderApp model)
    {
        try
        {
            string message;
            bool success;
            int customerID;

            (success, message, customerID) = await _menuApp.UpdateCustomerDetails(model);
            
            return Json(new { success = success, message = message, customerId = customerID });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while Save Customer Details.");
            return Json(new { success = false });
        }
    }

    // get complete order partial view---------------------
    [HttpGet]
    public IActionResult CompleteOrder(int orderId)
    {
        try
        {
            CompleteOrderApp model = new CompleteOrderApp
            {
                OrderId = orderId
            };
            return PartialView("_PartialCompleteModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while get Complete Order.");
            return BadRequest();
        }
    }
    public IActionResult IsOrderCompletedToServed(int orderId)
    {
        try
        {
            bool isServed = _menuApp.IsOrderCompletedToServed(orderId);
            return Json(new { success = isServed });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while Is Order Completed To Served.");
            return Json(new { success = false });
        }
    }
    [HttpPost]
    public async Task<IActionResult> FinalCompleteOrderApp (CompleteOrderApp model)
    {
        try
        {
            bool status;
            model.By= getUserEmail();
            status= await _menuApp.CompleteOrder(model);
            if(status)
            {
                TempData["success"] = "Order Completed Successfully";
            }
            var dashboard = await _dashboard.GetDashboardData(DashboardConst.CURRENT_MONTH, "", "");
            int updatedCount = dashboard.DashboardData!.TotalOrder;
            await _hubContext.Clients.All.SendAsync("TotalOrderAuto", updatedCount);
            int updatedSalesCount = dashboard.DashboardData!.TotalSales;
            await _hubContext.Clients.All.SendAsync("TotalSalesAuto", updatedSalesCount);
            int updateAvgOrderCount = dashboard.DashboardData.AvgOrderValue;
            await _hubContext.Clients.All.SendAsync("AvgOrderAuto", updateAvgOrderCount);
            double? updateAvgWaitingTime = dashboard.DashboardData.AvgWaitingTime;
            await _hubContext.Clients.All.SendAsync("AvgWaitingTimeAuto", updateAvgWaitingTime);
            int customerCount = dashboard.DashboardData.TotalCustomer;
            await _hubContext.Clients.All.SendAsync("TotalCustomerAuto", customerCount);

            return Json(new { success = status });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while Final Complete Order App.");
            return Json(new { success = false });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CancelOrderApp(int orderId)
    {
        try
        {
            string by= getUserEmail();
            bool success= await _menuApp.CancelOrderApp(orderId,by);
            return Json(new{success = success});
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while Cancel Order App.");
            return Json(new { success = false });
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
