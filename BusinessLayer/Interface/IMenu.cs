using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;

namespace BusinessLayer.Interface;

public interface IMenu
{
    List<Categories> GetCategories();

    void AddCategory(Category category);

    Categories GetCategoryById(int id); // Get category by ID

    void UpdateCategory(Category category); // Update category

    bool SoftDeleteCategory(int categoryId);

    Pagination<ItemsView> GetItemsByCategory(int categoryId, int PageNumber = 1, int PageSize = 5, string SearchKey = "");

    List<ItemsUnit> GetUnits();
    public ModifierGroupDetails GetModifierGroupDetails(int modifierGroupId, int itemId = -1);
    Task<string> AddItems(AddItemsViewModel model);

    AddItemsViewModel GetItemById(int id);

    Task<bool> UpdateItem(AddItemsViewModel item);

    bool SoftDeleteItems(List<int> itemIds);

    //---------- Modifier Group------------
    List<ModifierGroupModel> GetModifierGroups();

    void AddModifier(Modifiersgroup modifiersgroup);

    ModifierGroupModel GetModifierById(int id);

    void UpdateModifier(Modifiersgroup modifiersgroup, List<int> existingModifiers);
    bool SoftDeleteModfierGroup(int modifiergroupid);

    Pagination<ModifierItemViewModel> GetModifierItemsByModifierGroup(int modifiergroupid = -1, int PageNumber = 1, int PageSize = 5, string SearchKey = "");

    Pagination<ModifierItemViewModel> GetAllModifierItems(int PageNumber = 1, int PageSize = 5, string SearchKey = "");

    int GetModifierGroupIdByName(string name);

    void AddModifierGroupItemMapping(int modifierGroupId, List<int> modifierItemId);

    public int AddModifierItem(AddEditModifierItemViewModel model);

    public AddEditModifierItemViewModel GetModifierItemById(int id);
    void UpdateModifierItem(AddEditModifierItemViewModel model);
    bool SoftDeleteModifierItems(List<int> modifieritemIds , int mgId);

}
