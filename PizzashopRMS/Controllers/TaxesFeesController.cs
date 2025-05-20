using BusinessLayer.Interface;
using DataAccessLayer.Constants;
using DataAccessLayer.ViewModels;
using Microsoft.AspNetCore.Mvc;
using PizzashopRMS.Helpers;

namespace PizzashopRMS.Controllers;

[CustomAuthorise(new string[] { "admin","account manager" })]
public class TaxesFeesController : BaseController
{
    private readonly ITaxesFees _taxes;
    private readonly ILogger<TaxesFeesController> _logger;

    public TaxesFeesController(ITaxesFees taxes, ILogger<TaxesFeesController> logger): base(PermissionConst.TAXFEES)
    {
        _taxes = taxes;
        _logger= logger;
    }

    public IActionResult TaxesFeesView()
    {
        try
        {
            // Fetch data using the repository
            var taxesFeesViewModel = new TaxesFeesViewModel
            {
                TaxesViewModal = _taxes.GetAllTaxesFees()
            };

            return View(taxesFeesViewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading TaxesFeesView");
            TempData["error"] = "Failed to load tax and fee data.";
            return RedirectToAction("DashboardView", "Dashboard");
        }
    }
    public IActionResult TaxesFeesTableView(string SearchKey="")
    {
        try
        {
            // Fetch data using the repository
            var taxesFeesViewModel = new TaxesFeesViewModel
            {
                TaxesViewModal = _taxes.GetAllTaxesFees(SearchKey)
            };

            return PartialView("_PartialTaxesFees", taxesFeesViewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading TaxesFeesTableView with SearchKey: {SearchKey}", SearchKey);
            return PartialView("_PartialTaxesFees", new TaxesFeesViewModel());
        }
    }

    [HttpPost]
    public IActionResult AddTax(TaxesFeesViewModel model)
    {
        try
        {

            if (ModelState.IsValid)
            {
                var addTaxe = model.AddEditTaxe;
                var email = _taxes.GetEmailFromToken(Request);
                var result = _taxes.AddTax(addTaxe, email);
                if (result)
                {
                    TempData["success"] = "Tax added successfully!";
                }
                else
                {
                    TempData["error"] = "Failed to add tax because it already exists.";
                }
            }
            else
            {
                TempData["error"] = "Please correct the errors in the form.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding tax");
            TempData["error"] = "An unexpected error occurred while adding tax.";
        }

        return RedirectToAction("TaxesFeesView");
    }

    [HttpGet]
    public IActionResult GetTaxDetails(int id)
    {
        var taxDetails = _taxes.GetTaxById(id);
        if (taxDetails == null)
        {
            return NotFound("Tax details not found");
        }
        return PartialView("_PartialEditModal", taxDetails);
    }

    [HttpPost]
    public IActionResult EditTax(TaxesFeesViewModel model)
    {
        try
        {

            if (!ModelState.IsValid)
            {
                return View("_PartialEditModal", model);
            }

            bool success = _taxes.UpdateTax(model);
            if (success)
            {
                TempData["success"] = "Tax updated successfully!";
            }
            else
            {
                TempData["error"] = "Error occur while updation tax";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tax with ID: {Id}", model?.AddEditTaxe?.TaxesId);
            TempData["error"] = "An unexpected error occurred while updating tax.";
        }

        return RedirectToAction("TaxesFeesView");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteTaxes(int taxId)
    {
        try
        {
            await _taxes.DeleteTax(taxId);

            // Re-render the table partial view after deletion
            var taxesFeesViewModel = new TaxesFeesViewModel
            {
                TaxesViewModal = _taxes.GetAllTaxesFees()
            };

            return PartialView("_PartialTaxesFees", taxesFeesViewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tax with ID: {TaxId}", taxId);
            return StatusCode(500, "An error occurred while deleting the tax.");
        }
    }

}
