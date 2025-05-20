using System.Data.Entity.Infrastructure;
using BusinessLayer.Interface;
using DataAccessLayer.Constants;
using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PizzashopRMS.Helpers;

namespace PizzashopRMS.Controllers;

[CustomAuthorise(new string[] { "admin","account manager" })]
public class MenuController : BaseController
{
    private readonly IMenu _menu;

    // Constructor to initialize menu service
    public MenuController(IMenu menu) : base(PermissionConst.MENU)
    {
        _menu = menu;
    }

    // Displays the menu view with categories, items, and units
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult MenuView(int categoryId = -1 )
    {
        var model = new MenuViewModel();
        model.Categories = _menu.GetCategories();
        if (categoryId == -1) model.Items = _menu.GetItemsByCategory(model.Categories[0].CategoryId); // Pass items for a default category or all items
        else model.Items = _menu.GetItemsByCategory(categoryId); // Pass items for a default category or all items
        model.ItemsUnit = _menu.GetUnits();
        model.ModifierGroupModel = _menu.GetModifierGroups();
        model.ModifierItemViewModel = _menu.GetModifierItemsByModifierGroup(model.ModifierGroupModel[0].ModifierGroupId); // Pass items for a default category or all items
        model.ModifierItemAll = _menu.GetAllModifierItems();
        return View(model);
    }

    // Adds a new category
    [HttpPost]
    public IActionResult AddCategory(Category category)
    {
        if (!string.IsNullOrEmpty(category.Categoryname) && !string.IsNullOrEmpty(category.Categorydescription))
        {
            try
            {
                _menu.AddCategory(category);
                TempData["success"] = "Category added successfully!";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message; // Shows the toast message if category exists
            }

            return RedirectToAction("MenuView");
        }

        TempData["error"] = "Category name and description cannot be empty!";
        return View("MenuView");
    }

    // Fetches category details for editing
    [HttpGet]
    public IActionResult EditCategory(int id)
    {
        var category = _menu.GetCategoryById(id);
        if (category == null)
        {
            return Json(null);
        }
        return Json(new
        {
            categoryid = category.CategoryId,
            categoryname = category.Categoryname,
            categorydescription = category.Categorydescription
        });
    }

    // Updates an existing category
    [HttpPost]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult UpdateCategory(Categories AddEditCategory)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var updatedCategory = new Category
                {
                    Categoryid = AddEditCategory.CategoryId,
                    Categoryname = AddEditCategory.Categoryname,
                    Categorydescription = AddEditCategory.Categorydescription
                };

                _menu.UpdateCategory(updatedCategory);
                TempData["success"] = "Category updated successfully!";
            }
            catch (Exception)
            {
                TempData["error"] = "An error occurred while updating the category.";
            }
        }
        else
        {
            TempData["error"] = "Invalid data. Please check the inputs.";
        }

        return RedirectToAction("MenuView");
    }

    // Soft deletes a category
    [HttpPost]
    public IActionResult DeleteCategory(int categoryId)
    {
        bool isDeleted = _menu.SoftDeleteCategory(categoryId);
        if (isDeleted)
        {
            TempData["success"] = "Category deleted successfully!";
            return Json(new { success = true });
        }
        else
        {
            TempData["error"] = "Failed to delete category!";
            return Json(new { success = false });
        }
    }

    // Retrieves items based on selected category
    public IActionResult GetItemsByCategory(int categoryId, int PageNumber = 1, int PageSize = 5, string SearchKey = "")
    {
        var model = new MenuViewModel
        {
            Categories = _menu.GetCategories(),
            Items = _menu.GetItemsByCategory(categoryId, PageNumber, PageSize, SearchKey),
            ItemsUnit = _menu.GetUnits(),
            ModifierGroupModel = _menu.GetModifierGroups(),
        };
        return PartialView("~/Views/Menu/_PartialItems.cshtml", model);
    }
    [HttpGet]
    public IActionResult GetModifierGroups()
    {
        try
        {
            var modifierGroups = _menu.GetModifierGroups();

            if (modifierGroups == null || !modifierGroups.Any())
            {
                return Json(new { success = false, message = "No modifier groups found." });
            }

            return Json(new { success = true, data = modifierGroups });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error: " + ex.Message });
        }
    }


    // Displays the view for adding new items
    [HttpGet]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult AddItemsView()
    {
        return PartialView("~/Views/Menu/_PartialItems.cshtml");
    }

    [HttpGet]
    public IActionResult GetModifierGroupDetails(int modifierGroupId, int itemId = -1)
    {
        var modifierGroupDetails = _menu.GetModifierGroupDetails(modifierGroupId, itemId);

        if (modifierGroupDetails == null)
        {
            return NotFound();
        }

        return PartialView("_ModGroupDetail", modifierGroupDetails);
    }

    // Adds a new item 
    [HttpPost]
    public async Task<JsonResult> AddItemsPost(AddItemsViewModel AddEditItem)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data. Please check the inputs." });
            }

            // Process Modifier Group Data from JSON
            if (!string.IsNullOrEmpty(Request.Form["AddModGroupWithItems"]))
            {
                AddEditItem.AddModGroupWithItems = JsonConvert.DeserializeObject<List<AddModGroupWithItem>>(Request.Form["AddModGroupWithItems"]);
            }

            var successMessage = await _menu.AddItems(AddEditItem);
            TempData["success"] = "Item added successfully!";
            return Json(new { success = true, redirectUrl = "/Menu/MenuView" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "An error occurred while adding the item." });
        }
    }


    // Fetches item details for editing
    [HttpGet]
    public IActionResult GetItemById(int id)
    {
        var item = _menu.GetItemById(id);
        if (item == null)
        {
            return NotFound();
        }
        return Json(item);
    }

    // Updates an existing item
    [HttpPost]
    public async Task<IActionResult> EditItems(AddItemsViewModel AddEditItem)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Invalid input data" });
        }
        try
        {    // Process Modifier Group Data from JSON
            if (!string.IsNullOrEmpty(Request.Form["AddModGroupWithItems"]))
            {
                AddEditItem.AddModGroupWithItems = JsonConvert.DeserializeObject<List<AddModGroupWithItem>>(Request.Form["AddModGroupWithItems"]);
            }
            bool isUpdated = await _menu.UpdateItem(AddEditItem);

            if (isUpdated)
            {
                TempData["success"] = "Item Updata successfully";
                return Json(new { success = true, url = $"/Menu/MenuView?categoryId={AddEditItem.CategoryId}" });
            }
            else
            {
                TempData["error"] = "Item not found or could not be updated.";
                return Json(new { success = false, url = $"/Menu/MenuView?categoryId={AddEditItem.CategoryId}" });
            }
        }
        catch (Exception ex)
        {
            TempData["error"] = "An error occurred: " + ex.Message;
            return Json(new { success = false, url = $"/Menu/MenuView?categoryId={AddEditItem.CategoryId}" });

        }


    }

    // Soft deletes a item
    [HttpPost]
    public IActionResult DeleteItem(List<int> itemIds)
    {
        bool isDeleted = _menu.SoftDeleteItems(itemIds);
        if (isDeleted)
        {
            return Json(new { success = true });
        }
        else
        {
            return Json(new { success = false });
        }
    }


    [HttpPost]
    public IActionResult AddModifierGroup(Modifiersgroup AddModifierGroup)
    {
        if (!string.IsNullOrEmpty(AddModifierGroup.Modifiersgroupname) && !string.IsNullOrEmpty(AddModifierGroup.Modifiersgroupdescription))
        {
            var newModifier = new Modifiersgroup
            {
                Modifiersgroupname = AddModifierGroup.Modifiersgroupname,
                Modifiersgroupdescription = AddModifierGroup.Modifiersgroupdescription
            };

            _menu.AddModifier(newModifier);

            var modlist = new List<int>();

            // Fetch the newly created ModifierGroupId
            var modifierGroupId = _menu.GetModifierGroupIdByName(AddModifierGroup.Modifiersgroupname);
            string SelectedModifierIds = Request.Form["SelectedAddModifierIds"];
            if (!string.IsNullOrEmpty(SelectedModifierIds))
            {
                modlist = JsonConvert.DeserializeObject<List<int>>(SelectedModifierIds);
            }
            // If selected modifier items exist, save them in the mapping table
            // if (!string.IsNullOrEmpty(SelectedModifierIds))
            // {
            // var modifierIds = SelectedModifierIds.Split(',').Select(int.Parse).ToList();

            _menu.AddModifierGroupItemMapping(modifierGroupId, modlist);

            // }

            TempData["success"] = "Modifier Group and items added successfully!";
            return Json(null);

        }

        return View("MenuView");
    }

    // Edit modifier group
    [HttpGet]
    public IActionResult EditModifierGroup(int id)
    {
        var modifier = _menu.GetModifierById(id);
        if (modifier == null)
        {
            return Json(null);
        }

        return Json(new
        {
            modifiergroupid = modifier.ModifierGroupId,
            modifiergroupname = modifier.ModifierGroupName,
            modifiergroupdescription = modifier.ModifierGroupDescription,
            existingModifiers = modifier.ExistingModifiers.Select(x => new
            {
                ModifierId = x.ModifierItemId,
                ModifierName = x.ModifierItemName
            })
        });
    }

    [HttpPost]
    public IActionResult UpdateModifierGroup(ModifierGroupModel EditModifierGroup, string SelectedEditModifierIds)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Update Modifier Group details
                var updatedModifier = new Modifiersgroup
                {
                    Modifiersgroupid = EditModifierGroup.ModifierGroupId,
                    Modifiersgroupname = EditModifierGroup.ModifierGroupName,
                    Modifiersgroupdescription = EditModifierGroup.ModifierGroupDescription
                };

                // Parse the JSON string of modifier IDs into a list of integers
                var existingModifierIds = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(SelectedEditModifierIds);

                // Update the Modifier Group and its mappings
                _menu.UpdateModifier(updatedModifier, existingModifierIds);

                TempData["success"] = "Modifier Group updated successfully!";
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                Console.WriteLine(ex.Message);
                TempData["error"] = "An error occurred while updating the Modifier Group.";
            }
        }
        else
        {
            TempData["error"] = "Invalid data. Please check the inputs.";
        }

        return RedirectToAction("MenuView");
    }


    // Soft deletes a category
    [HttpPost]
    public IActionResult DeleteModfierGroup(int modifiergroupid)
    {
        bool isDeleted = _menu.SoftDeleteModfierGroup(modifiergroupid);
        if (isDeleted)
        {
            TempData["success"] = "Modifier Group deleted successfully!";
            return Json(new { success = true });
        }
        else
        {
            TempData["error"] = "Failed to delete Modifier Group!";
            return Json(new { success = false });
        }
    }

    // Retrieves items based on selected modifier group
    //pwd
    public IActionResult GetModifierItemsByModifierGroup(int modifiergroupid, int PageNumber = 1, int PageSize = 5, string SearchKey = "")
    {
        Console.WriteLine("qqqqqqqq"+modifiergroupid);
        var model = new MenuViewModel
        {
            ModifierGroupModel = _menu.GetModifierGroups(),
            ModifierItemViewModel = _menu.GetModifierItemsByModifierGroup(modifiergroupid, PageNumber, PageSize, SearchKey),
            ItemsUnit = _menu.GetUnits()
        };
        return PartialView("~/Views/Menu/_PartialModifier.cshtml", model);
    }

    //Controller for add modifier group add existing
    public IActionResult GetModifierItemsAllByModifierGroup(int PageNumber = 1, int PageSize = 5, string SearchKey = "")
    {
        var model = new MenuViewModel
        {

            ModifierItemAll = _menu.GetAllModifierItems(PageNumber, PageSize, SearchKey),

        };
        return PartialView("~/Views/Menu/_ExistingModifierItemModal.cshtml", model);
    }

    //Controller for edit modifier group add existing
    public IActionResult GetEditModifierItemsAllByModifierGroup(int PageNumber = 1, int PageSize = 5, string SearchKey = "")
    {
        var model = new MenuViewModel
        {

            ModifierItemAll = _menu.GetAllModifierItems(PageNumber, PageSize, SearchKey),

        };
        return PartialView("~/Views/Menu/_EditExistingModifierItemModal.cshtml", model);
    }

    //add modifier item
    [HttpPost]
    public IActionResult AddModifierItem([FromForm] MenuViewModel model, [FromForm] List<int> ModifierGroupIds)
    {
        AddEditModifierItemViewModel m = model.AddEditModItem;
        if (m == null || string.IsNullOrEmpty(m.ModifierItemName))
        {
            return BadRequest("Invalid data.");
        }

        try
        {
            // Assign ModifierGroupIds correctly
            m.ModifierGroupIds = ModifierGroupIds ?? new List<int>();

            if (!m.ModifierGroupIds.Any())
            {
                return BadRequest("ModifierGroupIds are required.");
            }

            int itemId = _menu.AddModifierItem(m);
            return Json(new { success = true, message = "Modifier Items Added successfully" });
        }
        catch (Exception ex)
        {
            TempData["error"] = "Error while adding modifier item !";
            return Json(new { success = false, message = "Error adding Modifier Item: " + ex.Message });
        }
    }

    //Get modifier item for edit modifier item
    [HttpGet]
    public IActionResult GetModifierItemDetails(int id)
    {

        var modifierItem = _menu.GetModifierItemById(id);

        if (modifierItem == null)
        {
            return NotFound("Modifier item not found.");
        }
        var viewModel = new MenuViewModel
        {
            AddEditModItem = modifierItem,
            ItemsUnit = _menu.GetUnits(),
            ModifierGroupModel = _menu.GetModifierGroups()

        };

        return PartialView("_EditModifierItemModal", viewModel);
    }

    //Update Modifier item
    [HttpPost]
    public IActionResult UpdateModifierItem(MenuViewModel model, [FromForm] List<int> ModifierGroupIds)
    {
        Console.WriteLine("Received ModifierGroupIds: " + string.Join(",", ModifierGroupIds));

        AddEditModifierItemViewModel m = model.AddEditModItem;
        if (m == null || m.ModifierItemId == 0)
        {
            return BadRequest("Invalid data.");
        }

        try
        {
            m.ModifierGroupIds = ModifierGroupIds ?? new List<int>();
            _menu.UpdateModifierItem(m);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // Soft deletes a category
    [HttpPost]
    public IActionResult DeleteModifierItem(List<int> modifieritemIds, int mgId)
    {
        bool isDeleted = _menu.SoftDeleteModifierItems(modifieritemIds, mgId);
        if (isDeleted)
        {
            return Json(new { success = true });
        }
        else
        {
            return Json(new { success = false });
        }
    }


}

