using BusinessLayer.Interface;
using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;

namespace BusinessLayer.Repository;

public class MenuRepository : IMenu
{
    private readonly PizzaShopContext _db;
    private readonly IUserList _userListRepository;

    // Constructor to initialize database context and user repository
    public MenuRepository(PizzaShopContext db, IUserList userListRepository)
    {
        _db = db;
        _userListRepository = userListRepository;
    }

    // Retrieves a list of active categories sorted by name
    public List<Categories> GetCategories()
    {
        return _db.Categories
            .Where(c => c.Isdeleted != true)
            .OrderBy(c => c.Categoryname)
            .Select(c => new Categories
            {
                CategoryId = c.Categoryid,
                Categoryname = c.Categoryname,
                Categorydescription = c.Categorydescription
            })
            .ToList();
    }

    // Retrieves all available item units sorted by name
    public List<ItemsUnit> GetUnits()
    {
        return _db.ItemsUnits
            .OrderBy(i => i.Unitname)
            .Select(i => new ItemsUnit
            {
                Unitid = i.Unitid,
                Unitname = i.Unitname,

            })
          .ToList();

    }

    public void AddCategory(Category category)
    {
        var existingCategory = _db.Categories.FirstOrDefault(c => c.Categoryname.ToLower().Trim() == category.Categoryname.ToLower().Trim());

        if (existingCategory != null)
        {
            if ((bool)existingCategory.Isdeleted!)
            {
                existingCategory.Isdeleted = false;
                existingCategory.Categorydescription = category.Categorydescription;
                _db.SaveChanges();
            }
            else
            {
                throw new Exception("Category name already exists!");
            }
        }
        else
        {
            var newCategory = new Category
            {
                Categoryname = category.Categoryname,
                Categorydescription = category.Categorydescription,
                Isdeleted = false
            };

            _db.Categories.Add(newCategory);
            _db.SaveChanges();
        }
    }

    // Retrieves a specific category by its ID
    public Categories GetCategoryById(int id)
    {
        var category = _db.Categories.FirstOrDefault(c => c.Categoryid == id);
        if (category == null) throw new KeyNotFoundException($"Category with ID {id} not found.");

        return new Categories
        {
            CategoryId = category.Categoryid,
            Categoryname = category.Categoryname,
            Categorydescription = category.Categorydescription
        };
    }

    // Updates an existing category in the database
    public void UpdateCategory(Category category)
    {
        var existingCategory = _db.Categories.FirstOrDefault(c => c.Categoryid == category.Categoryid);
        if (existingCategory != null)
        {
            existingCategory.Categoryname = category.Categoryname;
            existingCategory.Categorydescription = category.Categorydescription;
            _db.SaveChanges();
        }
    }

    // Performs a soft delete on a category by marking it as deleted
    public bool SoftDeleteCategory(int categoryId)
    {
        var category = _db.Categories.FirstOrDefault(c => c.Categoryid == categoryId);
        if (category != null)
        {
            category.Isdeleted = true;
            _db.SaveChanges();
            return true;
        }
        return false;
    }

    // Retrieves all items in a specific category
    public Pagination<ItemsView> GetItemsByCategory(int categoryId, int PageNumber = 1, int PageSize = 5, string SearchKey = "")
    {
        Pagination<ItemsView> newmodel = new Pagination<ItemsView>();
        var query = _db.Items
             .Where(i => i.Categoryid == categoryId && i.Isdeleted != true && (i.Itemname.ToLower().Contains(SearchKey.ToLower()) || i.Itemdescription.ToLower().Contains(SearchKey.ToLower())))
             .Select(i => new ItemsView
             {
                 ItemId = i.Itemid,
                 Itemname = i.Itemname,
                 Rate = i.Rate,
                 Itemtype = i.Itemtype,
                 Quantity = i.Quantity,
                 Isavailable = i.Isavailable,
                 Itemdescription = i.Itemdescription,
                 Itemimage = i.Itemimage,
                 Categoryid = i.Categoryid
             });
        if (PageNumber < 1)
        {
            PageNumber = 1;
        }
        newmodel.Count = query.Count();
        newmodel.MaxPage = (int)Math.Ceiling(newmodel.Count / (double)PageSize);

        newmodel.PageSize = PageSize;

        if (PageNumber > newmodel.MaxPage)
        {
            PageNumber = newmodel.MaxPage;
        }
        if (PageNumber < 1)
        {
            PageNumber = 1;
        }
        newmodel.PageNumber = PageNumber;
        newmodel.ParentId = categoryId;
        newmodel.SearchKey = SearchKey;
        newmodel.List = query.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToList();
        return newmodel;
    }

    // Adds a new item to the menu if it does not already exist
    public async Task<string> AddItems(AddItemsViewModel model)
    {
        Item Uniqueitem = _db.Items.FirstOrDefault(i => i.Itemname == model.Itemname);
        if (Uniqueitem != null)
        {
            return "Item already exists!";
        }
        var item = new Item
        {
            Itemname = model.Itemname,
            Rate = model.Rate,
            Itemtype = model.Itemtype,
            Quantity = model.Quantity,
            Isavailable = model.Isavailable,
            Itemdescription = model.Itemdescription,
            Itemimage = await _userListRepository.UploadPhotoAsync(model.Itemimage),
            Categoryid = model.CategoryId,
            Itemid = model.ItemId,
            Unitid = model.UnitId,
            Defaulttax = model.Defaulttax,
            Taxpercentage = model.Taxpercentage,
            Shortcode = model.Shortcode

        };
        _db.Add(item);
        await _db.SaveChangesAsync();

        if (model.AddModGroupWithItems != null && model.AddModGroupWithItems.Any())
        {
            var modGroupMappings = model.AddModGroupWithItems.Select(mg => new MapItemsModifiersgroup
            {
                Itemid = item.Itemid, // Use the newly inserted ItemId
                Modifiersgroupid = mg.ModifierGroupId,
                Minimum = mg.min,
                Maximum = mg.max
            }).ToList();

            _db.MapItemsModifiersgroups.AddRange(modGroupMappings);
            await _db.SaveChangesAsync();
        }
        return "Item added successfully!";
    }
    public ModifierGroupDetails GetModifierGroupDetails(int modifierGroupId, int itemId = -1)
    {

        Modifiersgroup? groupModel = _db.Modifiersgroups.Where(e => e.Modifiersgroupid == modifierGroupId).FirstOrDefault();
        ModifierGroupDetails model = new ModifierGroupDetails();
        if (groupModel != null)
        {
            model.ModifierGroupId = groupModel.Modifiersgroupid;
            model.ModifierGroupName = groupModel.Modifiersgroupname;
            model.ItemShows = (from mi in _db.Modifiers
                               join map in _db.MapModifiersgroupModifiers on mi.Modifiersid equals map.Modifiersid
                               where map.Modifiersgroupid == modifierGroupId
                               select new ItemShow
                               {
                                   ModifierItemName = mi.Modifiersname,
                                   Rate = (int)mi.Modifiersrate
                               }).ToList();

            if (itemId != -1)
            {
                MapItemsModifiersgroup item = _db.MapItemsModifiersgroups.Where(e => e.Modifiersgroupid == modifierGroupId && e.Itemid == itemId).FirstOrDefault();
                model.min = item.Minimum ?? 0;
                model.max = item.Maximum ?? 0;
            }
        }

        return model;
    }

    public AddItemsViewModel GetItemById(int id)
    {
        var item = _db.Items
            .Where(i => i.Itemid == id)
            .Select(i => new AddItemsViewModel
            {
                ItemId = i.Itemid,
                CategoryId = i.Categoryid,
                Itemname = i.Itemname,
                Itemtype = i.Itemtype,
                Rate = (int)i.Rate,
                Quantity = (int)i.Quantity,
                UnitId = i.Unitid,
                Isavailable = (bool)i.Isavailable,
                Defaulttax = (bool)i.Defaulttax,
                Taxpercentage = i.Taxpercentage,
                Shortcode = i.Shortcode,
                Itemdescription = i.Itemdescription,
                AddModGroupWithItems = _db.MapItemsModifiersgroups
                                     .Where(im => im.Itemid == id)
                                     .Select(im => new AddModGroupWithItem
                                     {
                                         ModifierGroupId = im.Modifiersgroupid,
                                         max = im.Maximum ?? 0,
                                         min = im.Minimum ?? 0
                                     }).ToList()
            })
            .FirstOrDefault();

        return item;
    }
    public async Task<bool> UpdateItem(AddItemsViewModel item)
    {
        var existingItem = await _db.Items.FindAsync(item.ItemId);
        if (existingItem == null)
            return false;

        // Update item properties
        existingItem.Categoryid = item.CategoryId;
        existingItem.Itemname = item.Itemname;
        existingItem.Itemtype = item.Itemtype;
        existingItem.Rate = item.Rate;
        existingItem.Quantity = item.Quantity;
        existingItem.Unitid = item.UnitId;
        existingItem.Isavailable = item.Isavailable;
        existingItem.Defaulttax = item.Defaulttax;
        existingItem.Taxpercentage = item.Taxpercentage;
        existingItem.Shortcode = item.Shortcode;
        existingItem.Itemdescription = item.Itemdescription;
        if (item.Itemimage != null)
        {
            existingItem.Itemimage = await _userListRepository.UploadPhotoAsync(item.Itemimage);
        }
        _db.Items.Update(existingItem);

        var existingMappings = _db.MapItemsModifiersgroups.Where(m => m.Itemid == item.ItemId);
        _db.MapItemsModifiersgroups.RemoveRange(existingMappings);

        // Add new modifier group mappings
        if (item.AddModGroupWithItems != null && item.AddModGroupWithItems.Any())
        {
            foreach (var modGroup in item.AddModGroupWithItems)
            {
                var newMapping = new MapItemsModifiersgroup
                {
                    Itemid = item.ItemId,
                    Modifiersgroupid = modGroup.ModifierGroupId,
                    Minimum = modGroup.min,
                    Maximum = modGroup.max
                };
                _db.MapItemsModifiersgroups.Add(newMapping);
            }
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public bool SoftDeleteItems(List<int> itemIds)
    {
        var items = _db.Items.Where(i => itemIds.Contains(i.Itemid)).ToList();
        var deletemap = _db.MapItemsModifiersgroups.Where(i => itemIds.Contains(i.Itemid)).ToList();
        _db.MapItemsModifiersgroups.RemoveRange(deletemap);
        if (items.Any())
        {
            foreach (var item in items)
            {
                item.Isdeleted = true;
            }
            _db.SaveChanges();
            return true;
        }
        return false;
    }

    public bool SoftDeleteModifierItems(List<int> modifieritemIds , int mgId)
    {
        if(mgId <= 0)
        {
            return false;
        }
        var deletemap = _db.MapModifiersgroupModifiers.Where(i => modifieritemIds.Contains(i.Modifiersid) & i.Modifiersgroupid== mgId).ToList();
        _db.MapModifiersgroupModifiers.RemoveRange(deletemap);
        _db.SaveChanges();
        return true;
    }

    public List<ModifierGroupModel> GetModifierGroups()
    {
        return _db.Modifiersgroups
             .Where(c => c.Isdeleted != true)
             .OrderBy(c => c.Modifiersgroupid)
             .Select(c => new ModifierGroupModel
             {
                 ModifierGroupId = c.Modifiersgroupid,
                 ModifierGroupName = c.Modifiersgroupname,
                 ModifierGroupDescription = c.Modifiersgroupdescription
             })
             .ToList();
    }
    // Adds a new modifier to the database
    public void AddModifier(Modifiersgroup modifiersgroup)
    {
        var existingCategory = _db.Modifiersgroups.FirstOrDefault(c => c.Modifiersgroupname.ToLower().Trim() == modifiersgroup.Modifiersgroupname.ToLower().Trim());
        if (existingCategory != null)
        {
            if ((bool)existingCategory.Isdeleted!)
            {
                existingCategory.Isdeleted = false;
                existingCategory.Modifiersgroupdescription = modifiersgroup.Modifiersgroupdescription;
                _db.SaveChanges();
            }
            else
            {
                throw new Exception("Modifier Group name already exists!");
            }
        }
        else
        {
            var newModifier = new Modifiersgroup
            {
                Modifiersgroupname = modifiersgroup.Modifiersgroupname,
                Modifiersgroupdescription = modifiersgroup.Modifiersgroupdescription,
                Isdeleted = false
            };
            _db.Modifiersgroups.Add(newModifier);
            _db.SaveChanges();
        }

    }

    public int GetModifierGroupIdByName(string name)
    {
        return _db.Modifiersgroups.Where(m => m.Modifiersgroupname == name)
                              .Select(m => m.Modifiersgroupid)
                              .FirstOrDefault();
    }
    public void AddModifierGroupItemMapping(int modifierGroupId, List<int> modifierItemId)
    {
        foreach (var modid in modifierItemId)
        {
            var mapping = new MapModifiersgroupModifier
            {
                Modifiersgroupid = modifierGroupId,
                Modifiersid = modid
            };

            _db.MapModifiersgroupModifiers.Add(mapping);

        }
        _db.SaveChanges();
    }

    public ModifierGroupModel GetModifierById(int id)
    {
        var modifierGroup = _db.Modifiersgroups
            .Where(e => e.Modifiersgroupid == id)
            .Select(modgroup => new ModifierGroupModel
            {
                ModifierGroupId = modgroup.Modifiersgroupid,
                ModifierGroupName = modgroup.Modifiersgroupname,
                ModifierGroupDescription = modgroup.Modifiersgroupdescription,
                ExistingModifiers = (from maptable in _db.MapModifiersgroupModifiers
                                     join moditem in _db.Modifiers on maptable.Modifiersid equals moditem.Modifiersid
                                     where maptable.Modifiersgroupid == id
                                     select new ExistingModifiersItemModel
                                     {
                                         ModifierItemId = moditem.Modifiersid,
                                         ModifierItemName = moditem.Modifiersname
                                     }).ToList()
            })
            .FirstOrDefault();

        return modifierGroup;
    }


    public ModifierGroupModel GetModifierByIdWithMappings(int id)
    {
        ModifierGroupModel modifierGroup = new ModifierGroupModel();
        Modifiersgroup modgroup = _db.Modifiersgroups.Where(e => e.Modifiersgroupid == id).FirstOrDefault();
        modifierGroup.ModifierGroupId = modgroup.Modifiersgroupid;
        modifierGroup.ModifierGroupName = modgroup.Modifiersgroupname;
        modifierGroup.ModifierGroupDescription = modgroup.Modifiersgroupdescription;

        modifierGroup.ExistingModifiers = (from maptable in _db.MapModifiersgroupModifiers
                                           join moditem in _db.Modifiers on maptable.Modifiersid equals moditem.Modifiersid
                                           where maptable.Modifiersgroupid == id
                                           select new ExistingModifiersItemModel
                                           {
                                               ModifierItemId = maptable.Modifiersid,
                                               ModifierItemName = moditem.Modifiersname
                                           }).ToList();

        if (modifierGroup == null) throw new KeyNotFoundException($"Modifier Group with ID {id} not found.");
        return modifierGroup;
    }

    public void UpdateModifier(Modifiersgroup modifiersgroup, List<int> existingModifiers)
    {
        var existingCategory = _db.Modifiersgroups.FirstOrDefault(c => c.Modifiersgroupid == modifiersgroup.Modifiersgroupid);
        if (existingCategory != null)
        {
            // Update Modifier Group details
            existingCategory.Modifiersgroupname = modifiersgroup.Modifiersgroupname;
            existingCategory.Modifiersgroupdescription = modifiersgroup.Modifiersgroupdescription;

            // Remove existing mappings
            var existingMappings = _db.MapModifiersgroupModifiers
                .Where(m => m.Modifiersgroupid == modifiersgroup.Modifiersgroupid)
                .ToList();

            if (existingMappings.Any())
            {
                _db.MapModifiersgroupModifiers.RemoveRange(existingMappings);
            }

            // Add new mappings
            if (existingModifiers != null && existingModifiers.Any())
            {
                foreach (var modifierId in existingModifiers)
                {
                    var newMapping = new MapModifiersgroupModifier
                    {
                        Modifiersgroupid = modifiersgroup.Modifiersgroupid,
                        Modifiersid = modifierId
                    };

                    _db.MapModifiersgroupModifiers.Add(newMapping);
                }
            }

            // Save changes
            _db.SaveChanges();
        }
    }

    // Performs a soft delete on a category by marking it as deleted
    public bool SoftDeleteModfierGroup(int modifiergroupid)
    {
        var modifiersgroup = _db.Modifiersgroups.FirstOrDefault(c => c.Modifiersgroupid == modifiergroupid);
        if (modifiersgroup != null)
        {
            modifiersgroup.Isdeleted = true;
            // Hard delete related records in mapping tables
            var modifierMappings = _db.MapModifiersgroupModifiers.Where(m => m.Modifiersgroupid == modifiergroupid);
            _db.MapModifiersgroupModifiers.RemoveRange(modifierMappings);
            var modifierBaseMappings = _db.MapItemsModifiersgroups.Where(mb => mb.Modifiersgroupid == modifiergroupid);
            _db.MapItemsModifiersgroups.RemoveRange(modifierBaseMappings);
            _db.SaveChanges();
            return true;
        }
        return false;
    }
    // Retrieves all items in a specific category
    public Pagination<ModifierItemViewModel> GetModifierItemsByModifierGroup(int modifiergroupid, int PageNumber = 1, int PageSize = 5, string SearchKey = "")
    {
        Pagination<ModifierItemViewModel> newmodel = new Pagination<ModifierItemViewModel>();


        var query = from map in _db.MapModifiersgroupModifiers
                    join modifier in _db.Modifiers on map.Modifiersid equals modifier.Modifiersid
                    join groupm in _db.Modifiersgroups on map.Modifiersgroupid equals groupm.Modifiersgroupid
                    join unit in _db.ItemsUnits on modifier.Modifiersunit equals unit.Unitid
                    where map.Modifiersgroupid == modifiergroupid && modifier.Isdeleted != true && groupm.Isdeleted != true && modifier.Modifiersname.ToLower().Contains(SearchKey.ToLower())
                    select new ModifierItemViewModel
                    {
                        ModifierItemId = modifier.Modifiersid,
                        ModifierGroupId = groupm.Modifiersgroupid,
                        ModifierItemName = modifier.Modifiersname,
                        Rate = (int)modifier.Modifiersrate,
                        ModifierItemDescription = modifier.Modifiersdescription,
                        Quantity = (int)modifier.Modifiersquantity,
                        // EditedBy = modifier.EditedBy,
                        // CreatedBy = modifier.CreatedBy,
                        // EditDate = modifier.EditDate,
                        // CreatedDate = modifier.CreatedDate,
                        ModifierUnitname = unit.Unitname,
                    };

        if (PageNumber < 1)
        {
            PageNumber = 1;
        }
        newmodel.Count = query.Count();
        newmodel.MaxPage = (int)Math.Ceiling(newmodel.Count / (double)PageSize);

        newmodel.PageSize = PageSize;

        if (PageNumber > newmodel.MaxPage)
        {
            PageNumber = newmodel.MaxPage;
        }
        if (PageNumber < 1)
        {
            PageNumber = 1;
        }
        newmodel.PageNumber = PageNumber;
        newmodel.ParentId = modifiergroupid;
        newmodel.SearchKey = SearchKey;
        newmodel.List = query.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToList();
        return newmodel;
    }
    public Pagination<ModifierItemViewModel> GetAllModifierItems(int PageNumber = 1, int PageSize = 5, string SearchKey = "")
    {
        Pagination<ModifierItemViewModel> newmodel = new Pagination<ModifierItemViewModel>();
        var query = from
            modifier in _db.Modifiers
                    join unit in _db.ItemsUnits on modifier.Modifiersunit equals unit.Unitid
                    where modifier.Isdeleted != true && modifier.Modifiersname.ToLower().Contains(SearchKey.ToLower())
                    select new ModifierItemViewModel
                    {
                        ModifierItemId = modifier.Modifiersid,
                        ModifierItemName = modifier.Modifiersname,
                        Rate = (int)modifier.Modifiersrate,
                        ModifierItemDescription = modifier.Modifiersdescription,
                        Quantity = (int)modifier.Modifiersquantity,
                        // EditedBy = modifier.EditedBy,
                        // CreatedBy = modifier.CreatedBy,
                        // EditDate = modifier.EditDate,
                        // CreatedDate = modifier.CreatedDate,
                        ModifierUnitname = unit.Unitname,
                    };
        if (PageNumber < 1)
        {
            PageNumber = 1;
        }
        newmodel.Count = query.Count();
        newmodel.MaxPage = (int)Math.Ceiling(newmodel.Count / (double)PageSize);

        newmodel.PageSize = PageSize;

        if (PageNumber > newmodel.MaxPage)
        {
            PageNumber = newmodel.MaxPage;
        }
        if (PageNumber < 1)
        {
            PageNumber = 1;
        }
        newmodel.PageNumber = PageNumber;
        newmodel.SearchKey = SearchKey;
        newmodel.List = query.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToList();
        return newmodel;

    }
    public int AddModifierItem(AddEditModifierItemViewModel model)
    {
        try
        {
            // Step 1: Save Modifier Item
            var modifierItem = new Modifier
            {
                Modifiersname = model.ModifierItemName,
                Modifiersrate = model.Rate,
                Modifiersquantity = model.Quantity,
                Modifiersdescription = model.ModifierItemDescription,
                Modifiersunit = model.Modifiersunit,
                CreatedBy = "Admin",
                EditDate = DateTime.Now
            };

            _db.Modifiers.Add(modifierItem);
            _db.SaveChanges(); // Save to get ModifierItemId

            // Step 2: Save Modifier Item - Modifier Group Mapping
            if (model.ModifierGroupIds != null && model.ModifierGroupIds.Any())
            {
                List<MapModifiersgroupModifier> mappings = model.ModifierGroupIds.Select(groupId => new MapModifiersgroupModifier
                {
                    Modifiersgroupid = groupId,
                    Modifiersid = modifierItem.Modifiersid
                }).ToList();

                _db.MapModifiersgroupModifiers.AddRange(mappings); // Use AddRange to optimize
                _db.SaveChanges();
            }

            return modifierItem.Modifiersid;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return 0; // Return 0 if insertion fails
        }
    }

    public AddEditModifierItemViewModel GetModifierItemById(int id)
    {
        var modifierItem = _db.Modifiers
            .Where(m => m.Modifiersid == id)
            .Select(m => new AddEditModifierItemViewModel
            {
                ModifierItemId = m.Modifiersid,
                ModifierItemName = m.Modifiersname,
                Rate = m.Modifiersrate ?? 0,
                Quantity = m.Modifiersquantity ?? 0,
                Modifiersunit = m.Modifiersunit,
                ModifierItemDescription = m.Modifiersdescription,
                ModifierGroupIds = _db.MapModifiersgroupModifiers
                                    .Where(mg => mg.Modifiersid == id)
                                    .Select(mg => mg.Modifiersgroupid)
                                    .ToList() ?? new List<int>() // Ensure ModifierGroupIds is not null
            }).FirstOrDefault();

        if (modifierItem == null)
        {
            throw new KeyNotFoundException($"Modifier item with ID {id} not found.");
        }

        return modifierItem;
    }
    public void UpdateModifierItem(AddEditModifierItemViewModel model)
    {
        if (model == null || model.ModifierItemId == 0)
            throw new ArgumentException("Invalid modifier item details");

        var modifierItem = _db.Modifiers.FirstOrDefault(m => m.Modifiersid == model.ModifierItemId);

        if (modifierItem == null)
            throw new Exception("Modifier item not found");

        // Update values
        modifierItem.Modifiersname = model.ModifierItemName;
        modifierItem.Modifiersrate = model.Rate;
        modifierItem.Modifiersquantity = model.Quantity;
        modifierItem.Modifiersdescription = model.ModifierItemDescription;
        modifierItem.Modifiersunit = model.Modifiersunit;
        modifierItem.EditDate = DateTime.Now;
        modifierItem.EditedBy = model.EditedBy;

        try
        {
            _db.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving modifier item: {ex.Message}");
            throw;
        }

        // Remove existing modifier group mappings
        var existingMappings = _db.MapModifiersgroupModifiers.Where(m => m.Modifiersid == model.ModifierItemId).ToList();

        try
        {
            if (existingMappings.Any())
            {
                _db.MapModifiersgroupModifiers.RemoveRange(existingMappings);
                _db.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing existing mappings: {ex.Message}");
            throw;
        }

        // Insert new mappings
        try
        {
            List<MapModifiersgroupModifier> newMappings = model.ModifierGroupIds.Select(groupId => new MapModifiersgroupModifier
            {
                Modifiersid = model.ModifierItemId,
                Modifiersgroupid = groupId
            }).ToList();

            _db.MapModifiersgroupModifiers.AddRange(newMappings);
            _db.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding new mappings: {ex.Message}");
            throw;
        }
    }


}
