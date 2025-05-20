namespace DataAccessLayer.Constants;

public class PermissionConst
{
    public const string USERS = "Users";
    public const string ROLEPERMISSION = "RoleAndPermission";
    public const string MENU = "Menu";
    public const string TABLESECTION = "TableAndSection";
    public const string TAXFEES = "TaxAndFee";
    public const string ORDERS = "Order";
    public const string CUSTOMERS = "Customers";


    // what permission for ----------------
    public const string CanView = "View";
    public const string CanAddEdit = "AddEdit";
    public const string CanDelete = "Delete";

    // Permission array get
    public static string[] GetPermissionNames()
    {
        return new string[]
        {
            USERS,
            ROLEPERMISSION,
            MENU,
            TABLESECTION,
            TAXFEES,
            ORDERS,
            CUSTOMERS
        };
    }

}
