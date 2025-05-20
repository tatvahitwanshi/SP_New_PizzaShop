using BusinessLayer.Interface;
using DataAccessLayer.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;

namespace PizzashopRMS.Controllers;

public class KotController :BaseOrderAppController
{
    private readonly IKot _kot;
    private readonly ILogger<KotController> _logger;
    public KotController(IKot kot , ILogger<KotController> logger) : base()
    {
        _kot = kot;
        _logger=logger;
    }

    public async Task<IActionResult> Kot()
    {
        try
        {
            KotViewModel model = new KotViewModel
            {
                CategoryList = await _kot.GetCategoryLists(),
            };
            return View(model);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error while Kot.");
            TempData["error"] = $"Failed to load KOT screen. Error: {ex.Message}";
            return View(new KotViewModel());            
        }
    }
    public async Task<IActionResult> GetTabContent(int id, string orderStatus = "In Progress")
    {
        try
        {
            ViewBag.CurrTab = id;
            KotViewModel model = new KotViewModel
            {
                CurrentCategory = id,
                OrderStatus = orderStatus,
                CategoryName = await _kot.GetCategoryName(id),
                Cards = _kot.GetOrderCards(id, orderStatus),
            };
            return PartialView("_PartialTabs", model);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error while GetTabContent.");
           return BadRequest(ex);          
        }
    }

    [HttpGet]
    public IActionResult ChangeQuantityModal(string orderValues, string orderStatus, int CurrentCategory = 0)
    {
        try
        {

            OrderCards ordercard = new OrderCards();
            if(!string.IsNullOrEmpty(orderValues))
            {
                ordercard = JsonConvert.DeserializeObject<OrderCards>(orderValues)!;
            }

            if(orderStatus == "In Progress") orderStatus = "Ready";
            else if(orderStatus == "Ready")
            {
                orderStatus = "In Progress";
                ViewBag.isServed= _kot.IsServed((int)ordercard.OrderId!);
            } 
                
            ViewBag.OrderStatus = orderStatus;
            ViewBag.CurrentCategory = CurrentCategory;
            return PartialView("_PartialChangeStatusModal", ordercard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while Change Quantity Modal.");
           return BadRequest(ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateChangeQuantity(OrderCards ordercard, string OrderStatus, int CurrentCategory )
    {
        try
        {
            await _kot.UpdateChangeQuantity(ordercard, OrderStatus);
            if(OrderStatus == "In Progress") OrderStatus = "Ready";
            else if(OrderStatus == "Ready") OrderStatus = "In Progress";
            return Json(new { id = CurrentCategory, orderStatus = OrderStatus });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while UpdateChangeQuantity.");
            return BadRequest(ex);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetPendingOrder()
    {
        try
        {
            List<PendingOrders> model = await _kot.GetPendingOrders();
            return PartialView("_PartialPendingOrders", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while UpdateChangeQuantity.");
            return BadRequest(ex);
        }
    }
    public async Task<IActionResult> ChangePendingToInProgress(string selectedOrderIds)
    {
        try
        {
            List<int> order;
            if(!string.IsNullOrEmpty(selectedOrderIds))
            {
                order = JsonConvert.DeserializeObject<List<int>>(selectedOrderIds)!;
                await _kot.SetToInProgress(order);
            }

            return Json(new {success = true});
        }
        catch (Exception)
        {
            return Json(new { success = false });
        }
    }
    public async Task<IActionResult> MarkOrderServed(int orderId)
    {
        try
        {
            bool status;
            string message;

            (status, message) = await _kot.MarkOrderServed(orderId);
            return Json(new { success = status, messsage = message });
        }
        catch (Exception ex)
        {
            TempData["error"] = $"Error marking order as served. Error: {ex.Message}";
            return Json(new { success = false, messsage = "Unexpected error occurred" });
        }
    }
}
