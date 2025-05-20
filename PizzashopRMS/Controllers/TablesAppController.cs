using BusinessLayer.Interface;
using DataAccessLayer.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml.Drawing.Chart;
using PizzashopRMS.Helpers;
namespace PizzashopRMS.Controllers;

[CustomAuthorise(new string[] { "admin","account manager" })]
public class TablesAppController : BaseOrderAppController
{
    private readonly ITablesApp _tablesApp;
    private readonly IWaitingList _waitingList;
    private readonly ILogger<TablesAppController> _logger;

    public TablesAppController(ITablesApp tablesApp , IWaitingList waitingList  , ILogger<TablesAppController> logger) : base()
    {
        _tablesApp = tablesApp;
        _waitingList= waitingList;
        _logger= logger;
    }
    public  async Task<IActionResult> TablesApp()
    {
        try
        {
            TablesOrderAppViewModel model = new TablesOrderAppViewModel
            {
                sectionList = await _tablesApp.getTableOrders()
            };
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading TablesApp view.");
            TempData["error"] = "Failed to load table view. Please try again.";
            return RedirectToAction("DashboardView", "Dashboard");
        }
    }

    [HttpGet]
    public async Task<IActionResult> LoadParialView()
    {
        try
        {
            List<SectionOrderView> model = await _tablesApp.getTableOrders();
            return PartialView("_SectionListPartial", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading _SectionListPartial.");
            return BadRequest();
        }
    }

    [HttpGet]   
    public async Task<IActionResult> GetCustomerList(int sectionId)
    {
        try
        {
            List<TokenDetail> model = await _waitingList.getTokenList(sectionId);
            return PartialView("_WaitingListTable", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading GetCustomerList.");
            return BadRequest();
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAssignTable(int sectionId)
    {
        try
        {
            WaitingToken token = new WaitingToken
            {
                SectionId = sectionId,
                SectionList = await _waitingList.getSectionList(),
            };
            return PartialView("_AssignTable", token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading GetAssignTable.");
            return BadRequest();
        }
    }

    [HttpPost]
    public async Task<IActionResult> AssignToken(WaitingToken token , string tableIds="[]")
    {
        try
        {
            List<int>? tablesAssigned = JsonConvert.DeserializeObject<List<int>>(tableIds);
            string message = string.Empty;
            bool status = true;
            token.By=getUserEmail();
            (status, message) = await _tablesApp.assignToken(token, tablesAssigned);
            return Json(new{status = status, message=message ,sectionid=token.SectionId});
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message});
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning token for sectionId: {SectionId}", token.SectionId);
            return Json(new { status = false, message = "Failed to assign token. Please try again." });
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> DeleteToken(int tableId)
    {
        try
        {
            int TokenId = _tablesApp.getTokenId(tableId);

            if(TokenId != 0)
                await _waitingList.deleteToken(TokenId);

            return Json(new { status = true, message = tableId});
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Delete Token ");
            return Json(new { status = false, message = "Failed to delete token." });
        }
    }

}
