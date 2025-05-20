namespace DataAccessLayer.Constants;

public class DashboardConst
{
    public const string TODAY = "Today";
    public const string LAST_7_DAYS = "Last 7 Days";
    public const string LAST_30_DAYS = "Last 30 Days";
    public const string CURRENT_MONTH = "Current Month";
    public const string CUSTOM_DATE = "Custom date";
    
    
    public static string[] GetDashboardTime()
    {
        return new string[]
        {
            TODAY,
            LAST_7_DAYS,
            LAST_30_DAYS,
            CURRENT_MONTH,
            CUSTOM_DATE
            
        };
    }
}
