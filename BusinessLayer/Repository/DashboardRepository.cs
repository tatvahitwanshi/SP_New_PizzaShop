using BusinessLayer.Interface;
using DataAccessLayer.Constants;
using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Repository;

public class DashboardRepository : IDashboard
{
    private readonly PizzaShopContext _db;
    public DashboardRepository(PizzaShopContext db)
    {
        _db = db;

    }
    public async Task<DashboardViewModel> GetDashboardData(string TimeInterval, string StartDate, string EndDate)
    {
        DashboardViewModel objPass = new DashboardViewModel
        {
            StartDate = StartDate,
            EndDate = EndDate,
            TimeInterval = TimeInterval,
            StartDateTime = !string.IsNullOrEmpty(StartDate) ? DateTime.Parse(StartDate) : null,
            EndDateTime = !string.IsNullOrEmpty(EndDate) ? DateTime.Parse(EndDate) : null
        };

        switch (TimeInterval)
        {
            case DashboardConst.TODAY:
                objPass.StartDateTime = DateTime.Now.Date;
                objPass.EndDateTime = DateTime.Now.Date.AddDays(1).AddTicks(-1);
                break;
            case DashboardConst.LAST_7_DAYS:
                objPass.StartDateTime = DateTime.Now.Date.AddDays(-6);
                objPass.EndDateTime = DateTime.Now.Date.AddDays(1).AddTicks(-1);
                break;
            case DashboardConst.LAST_30_DAYS:
                objPass.StartDateTime = DateTime.Now.Date.AddDays(-29);
                objPass.EndDateTime = DateTime.Now.Date.AddDays(1).AddTicks(-1);
                break;
            case DashboardConst.CURRENT_MONTH:
                DateTime now = DateTime.Now;
                objPass.StartDateTime = new DateTime(now.Year, now.Month, 1);
                objPass.EndDateTime = objPass.StartDateTime.Value.AddMonths(1).AddTicks(-1);
                break;
            case DashboardConst.CUSTOM_DATE:
                break;
        }

        var completedOrders = _db.Orders
            .Where(o => o.CreatedDate.HasValue &&
                        o.CreatedDate.Value.Date >= objPass.StartDateTime!.Value.Date &&
                        o.CreatedDate.Value.Date <= objPass.EndDateTime!.Value.Date &&
                        o.Orderstatusid == 2)
            .ToList();

        //OrderCount--------------------------------------------------------------------------------------
        var orderCount = completedOrders.Count;

        //TotalSales---------------------------------------------------------------------------------------
        var totalSales = completedOrders.Sum(o => o.Totalamount ?? 0);

        //TotalOrderValue----------------------------------------------------------------------------------
        var totalOrderValue = orderCount > 0 ? totalSales / orderCount : 0;

        //WaitingListCount----------------------------------------------------------------------------------
        var waitingListCount = _db.WaitingTables
            .Where(c => c.CreatedDate.HasValue &&
                            c.CreatedDate.Value >= objPass.StartDateTime!.Value &&
                            c.CreatedDate.Value <= objPass.EndDateTime!.Value && c.Isassigned == false)
            .Count();

        //CustomerCount----------------------------------------------------------------------------------
        var customerCount = _db.Customers
            .Where(c => c.CreatedDate.HasValue &&
                        c.CreatedDate.Value >= objPass.StartDateTime!.Value &&
                        c.CreatedDate.Value <= objPass.EndDateTime!.Value)
            .Count();

        //Avg Waiting Time-------------------------------------------------------------------------------
       var avgWaitingTime = _db.Orders
            .Where(o =>
                (o.Orderstatusid == 2 || o.Orderstatusid == 4) &&
                o.CreatedDate != null &&
                o.Servetime != null &&
                o.CreatedDate.Value >= objPass.StartDateTime!.Value &&
                o.CreatedDate.Value <= objPass.EndDateTime!.Value)
            .ToList()
            .Select(o => (o.Servetime!.Value - o.CreatedDate!.Value).TotalMinutes)
            .DefaultIfEmpty(0)
            .Average();

        //Top And Least Selling Items-----------------------------------------------------------------------
       var itemSalesQuery = _db.Dishes
            .Where(d => d.Order.CreatedDate.HasValue &&
                        d.Order.CreatedDate.Value >= objPass.StartDateTime!.Value &&
                        d.Order.CreatedDate.Value <= objPass.EndDateTime!.Value)
            .Join(_db.Items,
                dish => dish.Itemid,
                item => item.Itemid,
                (dish, item) => new { dish, item })
            .GroupBy(g => new { g.item.Itemid, g.item.Itemname, g.item.Itemimage })
            .Select(g => new TopLeastSellingItem
            {
                ItemId = g.Key.Itemid,
                ItemName = g.Key.Itemname,
                ImageUrl = g.Key.Itemimage,
                OrderCount = g.Select(x => x.dish.Orderid).Distinct().Count()
            });
                var itemSales = itemSalesQuery.ToList();

        var topItems = itemSales
            .OrderByDescending(i => i.OrderCount)
            .Take(2)
            .ToList();

        var leastItems = itemSales
            .OrderBy(i => i.OrderCount)
            .Take(2)
            .ToList();

        objPass.DashboardData = new DashboardData
        {
            TotalOrder = orderCount,
            TotalSales = (int)totalSales,
            AvgOrderValue = (int)totalOrderValue,
            WaitingListCount = waitingListCount,
            TotalCustomer = customerCount,
            AvgWaitingTime= Math.Round(avgWaitingTime,2),
            TopSellingItems = topItems,
            LeastSellingItems = leastItems
        };

        //For Revenue----------------------------------------------------------------------------------
        // Get the full date range between StartDateTime and EndDateTime
        var allDatesInRange = Enumerable.Range(0, (objPass.EndDateTime!.Value.Date - objPass.StartDateTime!.Value.Date).Days + 1)
            .Select(offset => objPass.StartDateTime!.Value.Date.AddDays(offset))
            .ToList();

        //Get total revenue per date for completed orders
        var groupedRevenue = _db.Orders
            .Where(o => o.CreatedDate.HasValue &&
                        o.CreatedDate.Value.Date >= objPass.StartDateTime!.Value.Date &&
                        o.CreatedDate.Value.Date <= objPass.EndDateTime!.Value.Date &&
                        o.Orderstatusid == 2)
            .AsEnumerable()
            .GroupBy(o => o.CreatedDate!.Value.Date)
            .ToDictionary(g => g.Key, g => g.Sum(o => o.Totalamount ?? 0));

        //Map all dates to revenue, using 0 if not found
        objPass.DashboardData.RevenueByDate = allDatesInRange
            .Select(date => new RevenueChartData
            {
                Date = date.ToString("yyyy-MM-dd"),
                TotalRevenue = groupedRevenue.ContainsKey(date) ? groupedRevenue[date] : 0
            })
            .ToList();

        //For Customers----------------------------------------------------------------------------------
        // Group customer count by date
        var groupedCustomers = _db.Customers
            .Where(c => c.CreatedDate.HasValue &&
                        c.CreatedDate.Value.Date >= objPass.StartDateTime!.Value.Date &&
                        c.CreatedDate.Value.Date <= objPass.EndDateTime!.Value.Date)
            .AsEnumerable()
            .GroupBy(c => c.CreatedDate!.Value.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        // Map to full date range
        int prevCustomer =await _db.Customers.Where(e => e.CreatedDate.HasValue && e.CreatedDate.Value.Date < objPass.StartDateTime!.Value.Date).CountAsync();
        objPass.DashboardData.CustomerByDate = allDatesInRange
            .Select(date => 
            {
                prevCustomer += groupedCustomers.ContainsKey(date) ? groupedCustomers[date] : 0;
                return new CustomerChartData{
                    Date = date.ToString("yyyy-MM-dd"),
                    TotalCustomers = prevCustomer
                };
            })
            .ToList();

        return objPass;
    }


}
