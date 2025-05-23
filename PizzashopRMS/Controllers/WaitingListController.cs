using BusinessLayer.Interface;
using DataAccessLayer.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PizzashopRMS.Helpers;

namespace PizzashopRMS.Controllers;

[CustomAuthorise(new string[] { "admin","account manager" })]
public class WaitingListController : BaseOrderAppController
{
    public readonly IWaitingList _waitingList;
    private readonly ILogger<WaitingListController> _logger;
    private readonly IHubContext<HubSignalR> _hubContext;
    private readonly IDashboard _dashboard;


    public WaitingListController(IWaitingList waitingList, ILogger<WaitingListController> logger, IHubContext<HubSignalR> hubContext, IDashboard dashboard)
    {
        _waitingList = waitingList;
        _logger = logger;
        _hubContext = hubContext;
        _dashboard = dashboard;
    }

    public async Task<IActionResult> WaitingList()
    {
        try
        {
            WaitingListViewModel model = new WaitingListViewModel
            {
                CurrentId = 0,
                SectionList = await _waitingList.getSectionList(),
                TokenList = await _waitingList.getTokenList(0),
            };
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading WaitingList view.");
            TempData["error"] = "An error occurred while loading the waiting list.";
            return RedirectToAction("DashboardView", "Dashboard");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetTabContent(int id = 0)
    {
        try
        {
            WaitingListViewModel model = new WaitingListViewModel
            {
                CurrentId = id,
                SectionList = await _waitingList.getSectionList(),
                TokenList = await _waitingList.getTokenList(id),
            };
            return PartialView("_TabContent", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tab content for section ID {Id}", id);
            return BadRequest();
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetTableList(int sectionId, int capacity = 0)
    {
        try
        {
            List<TableSingle> tableList = await _waitingList.getTableList(sectionId, capacity);
            return Json(new { tableList });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching table list for section ID {SectionId} and capacity {Capacity}", sectionId, capacity);
            return BadRequest();
        }
    }

    [HttpGet]
    public async Task<IActionResult> GenerateToken(int id)
    {
        try
        {
            WaitingToken model = await _waitingList.getToken(id);
            return PartialView("_GenerateTokenModal", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading GenerateToken modal for ID {Id}", id);
            return BadRequest();
        }
    }

    [HttpPost]
    public async Task<IActionResult> GenerateToken(WaitingToken token)
    {
        token.By = getUserEmail();
        var message = string.Empty;

        if (ModelState.IsValid)
        {
            try
            {
                await _waitingList.generateToken(token);
                message = "Token generated successfully";

                var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var endDate = DateTime.Now;
                int updatedCount = await _dashboard.GetWaitingListCount(startDate, endDate);
                await _hubContext.Clients.All.SendAsync("WaitingListAuto", updatedCount);
                return Json(new { success = true, message = message });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating token.");
                return Json(new { success = false, message = "An unexpected error occurred." });
            }
        }

        message = "Please fill all required fields";
        return Json(new { success = false, message = message });
    }


    [HttpPost]
    public async Task<IActionResult> AssignTable(AssignToken assignToken)
    {
        try
        {
            if (assignToken.TokenId > 0 && assignToken.TableId > 0 && assignToken.SectionId > 0)
            {
                assignToken.By = getUserEmail();
                await _waitingList.assignToken(assignToken);
                return Json(new { success = true, message = "Table assigned successfully" });
            }
            return Json(new { success = false, message = "Invalid data submitted." });
        }
        catch (Exception ex)
        {
            // _logger.LogError(ex, "Error assigning table to token ID {TokenId}", assignToken?.TokenId);
            // return Json(new { success = false, message = "An error occurred while assigning the table." });
            _logger.LogError(ex, "Error assigning table to token ID {TokenId}", assignToken?.TokenId);
            var errorMessage = ex.InnerException?.Message ?? ex.Message;
            return Json(new { success = false, message = errorMessage });
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteToken(int tokenId)
    {
        try
        {
            if (tokenId > 0)
            {
                await _waitingList.deleteToken(tokenId);
                var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var endDate = DateTime.Now;
                int updatedCount = await _dashboard.GetWaitingListCount(startDate, endDate);
                await _hubContext.Clients.All.SendAsync("WaitingListAuto", updatedCount);
                return Json(new { success = true, message = "Waiting token deleted successfully" });
            }
            return Json(new { success = false, message = "Invalid token ID." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting token ID {TokenId}", tokenId);
            return Json(new { success = false, message = "An error occurred while deleting the token." });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomerFromEmail(string email)
    {
        try
        {
            var customer = await _waitingList.getCustomerFromEmail(email);
            if (customer != null)
            {
                return Json(new { success = true, customer });
            }
            return Json(new { success = false, message = "Customer not found." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching customer for email {Email}", email);
            return Json(new { success = false, message = "An error occurred while fetching customer details." });
        }
    }

}
