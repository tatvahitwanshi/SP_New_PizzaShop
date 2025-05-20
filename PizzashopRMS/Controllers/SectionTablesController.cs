using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BusinessLayer.Interface;
using DataAccessLayer.Constants;
using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;
using Microsoft.AspNetCore.Mvc;
using PizzashopRMS.Helpers;

namespace PizzashopRMS.Controllers;

[CustomAuthorise(new string[] { "admin" ,"account manager"}, PermissionConst.CanView, PermissionConst.TABLESECTION)]
public class SectionTablesController : BaseController
{
    private readonly ISectionTables _sectionTables;
    private readonly ILogger<SectionTablesController> _logger;


    // Constructor to initialize menu service
    public SectionTablesController(ISectionTables sectionTables,ILogger<SectionTablesController> logger) : base(PermissionConst.TABLESECTION)
    {
        _sectionTables = sectionTables;
        _logger=logger;
    }

    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult SectionTablesView(int sectionId = -1)
    {
        try
        {
            SectionTablesViewModal modal = new SectionTablesViewModal();
            modal.SectionViewModals = _sectionTables.GetSections();
            modal.AddEditSection = new AddEditSectionViewModal();
            // modal.TablesViews=_sectionTables.GetTableBySection();
            if (sectionId == -1) modal.TablesViews = _sectionTables.GetTableBySection(modal.SectionViewModals[0].SectionId);
            else modal.TablesViews = _sectionTables.GetTableBySection(sectionId);

            return View(modal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading SectionTablesView for sectionId: {SectionId}", sectionId);
            TempData["error"] = "Failed to load section tables. Please try again.";
            return RedirectToAction("DashboardView", "Dashboard");
        }
    }

    [HttpPost]
    public IActionResult AddSections(SectionTablesViewModal model)
    {
        if (model.AddEditSection != null &&
            !string.IsNullOrEmpty(model.AddEditSection.Sectionname) &&
            !string.IsNullOrEmpty(model.AddEditSection.Sectiondescription))
        {
            var email = _sectionTables.GetEmailFromToken(Request);

            if (email == null)
            {
                TempData["error"] = "User not authenticated!";
                return RedirectToAction("SectionTablesView");
            }

            var newSection = new Section
            {
                Sectionname = model.AddEditSection.Sectionname,
                Sectiondescription = model.AddEditSection.Sectiondescription
            };

            try
            {
                _sectionTables.AddSections(newSection, email);
                TempData["success"] = "Section added successfully!";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message; // Capture exception message for the toaster
            }

            return RedirectToAction("SectionTablesView");
        }

        TempData["error"] = "Invalid data provided!";
        return View("SectionTablesView");
    }

    // Fetches Section details for editing
    public IActionResult EditSection(int id)
    {
        var sections = _sectionTables.GetSectionById(id);
        if (sections == null)
        {
            return Json(null);
        }
        return Json(new
        {
            sectionid = sections.SectionId,
            sectionname = sections.Sectionname,
            sectiondescription = sections.Sectiondescription
        });
    }

    // Updates an existing category
    [HttpPost]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult UpdateSection(SectionTablesViewModal section)
    {
        AddEditSectionViewModal sec = section.AddEditSection;

        if (sec != null)
        {
            var email = _sectionTables.GetEmailFromToken(Request);
            try
            {
                var updatedSection = new Section
                {
                    Sectionid = sec.SectionId,
                    Sectionname = sec.Sectionname,
                    Sectiondescription = sec.Sectiondescription
                };

                _sectionTables.UpdateSection(updatedSection, email);
                TempData["success"] = "Section updated successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating section.");
                TempData["error"] = "An error occurred while updating the section.";
            }
        }
        else
        {
            TempData["error"] = "Invalid data for section. Please check the inputs.";
        }

        return RedirectToAction("SectionTablesView");
    }

    // Soft deletes a category
    [HttpPost]
    public IActionResult DeleteSection(int sectionId)
    {
        try
        {
            bool status = _sectionTables.SoftDeleteSection(sectionId);
            string message="";
            (message,status)= _sectionTables.GetMessageSection(sectionId);
            return Json(new { success = status , message= message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting section {SectionId}", sectionId);
            return Json(new { success = false, message = "Failed to delete section." });
        }
    }

    public IActionResult GetTablesBySection(int sectionId, int PageNumber = 1, int PageSize = 5, string SearchKey = "")
    {
        var modal = new SectionTablesViewModal
        {
            TablesViews = _sectionTables.GetTableBySection(sectionId, PageNumber, PageSize, SearchKey),
            SectionViewModals = _sectionTables.GetSections()
        };
        return PartialView("~/Views/SectionTables/_PartialTable.cshtml", modal);
    }

    [HttpPost]
    public IActionResult AddTable(SectionTablesViewModal model)
    {
        AddEditTablesView tablemodel = model.AddEditTables;

        if (tablemodel != null)
        {
            if (tablemodel.SectionId == null || tablemodel.SectionId == 0)
            {
                TempData["error"] = "SectionId is null or invalid!";
                return RedirectToAction("SectionTablesView" , new { sectionId = tablemodel.SectionId });
            }

            var email = _sectionTables.GetEmailFromToken(Request);
            var resultMessage = _sectionTables.AddTable(tablemodel, email);

            if (resultMessage == "Table added successfully!" || resultMessage == "Table restored successfully!")
            {
                TempData["success"] = resultMessage;
            }
            else
            {
                TempData["error"] = resultMessage; // This will display "Table name already exists!" as a toaster message.
            }

            return RedirectToAction("SectionTablesView", new { sectionId = tablemodel.SectionId });
        }

        TempData["error"] = "Validation failed. Please check the input fields.";
        model.SectionViewModals = _sectionTables.GetSections();
        model.TablesViews = _sectionTables.GetTableBySection(model.AddEditTables.SectionId ?? -1);
        return View("SectionTablesView", model);
    }

    public async Task<IActionResult> GetTableDetails(int tableId)
    {
        var tableDetails = await _sectionTables.GetTableByIdAsync(tableId);
        if (tableDetails == null)
        {
            return NotFound();
        }

        return Json(tableDetails);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateTable(SectionTablesViewModal model)
    {
        AddEditTablesView tablemodel = model.AddEditTables;
        if (tablemodel != null)
        {
            var isUpdated = _sectionTables.UpdateTable(tablemodel);
            if (isUpdated)
            {
                TempData["success"] = "Table updated successfully.";
                return RedirectToAction("SectionTablesView"); // Redirect back to your main view.
            }
            else
            {
                ModelState.AddModelError("", "Unable to update the table. Please try again.");
            }
        }

        TempData["error"] = "Validation failed. Please review the form.";
        return RedirectToAction("SectionTablesView");
    }


    // Soft deletes a table
    [HttpPost]
    public IActionResult DeleteTable(List<int> tableIds)
    {
        bool status = _sectionTables.SoftDeleteTable(tableIds);
        string message="";
        (message,status)= _sectionTables.GetMessage(tableIds);
        
        return Json(new { success = status , message= message });

    }

}
