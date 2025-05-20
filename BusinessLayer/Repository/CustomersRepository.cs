using BusinessLayer.Interface;
using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Repository;

public class CustomersRepository : ICustomers
{
    private readonly PizzaShopContext _db;
    public CustomersRepository(PizzaShopContext db)
    {
        _db = db;

    }
    public async Task<CustomerPage> GetCustomers(CustomersPaginationParams paramsModel)
    {
        CustomerPage model = new CustomerPage();

        var queryTemp = from customes in _db.Customers
                        where customes.Customername.ToLower().Contains(paramsModel.Searchkey.ToLower())
                        select new CustomerList
                        {
                            CustomerId = customes.Customerid,
                            CustomerName = customes.Customername,
                            CustomerEmail = customes.Customeremail,
                            PhoneNumber = customes.PhoneNumber,
                            LastOrder = (from orders in _db.Orders
                                         where orders.Customerid == customes.Customerid
                                         orderby orders.CreatedDate descending
                                         select orders.CreatedDate).FirstOrDefault(),
                            TotalOrder = (from orders in _db.Orders
                                          where orders.Customerid == customes.Customerid
                                          select orders).Count()
                        };

        DateTime? startDateTime = string.IsNullOrEmpty(paramsModel.StartDate) ? null : DateTime.Parse(paramsModel.StartDate);
        DateTime? endDateTime = string.IsNullOrEmpty(paramsModel.EndDate) ? null : DateTime.Parse(paramsModel.EndDate);

        switch (paramsModel.LastDays)
        {
            case "Custom":
                if (startDateTime != null)
                {
                    queryTemp = queryTemp.Where(o => o.LastOrder.HasValue && o.LastOrder.Value.Date >= startDateTime.Value.Date);
                    model.StartDate = startDateTime;
                }
                if (endDateTime != null)
                {
                    queryTemp = queryTemp.Where(o => o.LastOrder.HasValue && o.LastOrder.Value.Date <= endDateTime.Value.Date);
                    model.EndDate = endDateTime;
                }

                break;

            case "Last 7 Days":
                queryTemp = queryTemp.Where(o => o.LastOrder.HasValue && o.LastOrder.Value.Date >= DateTime.Now.AddDays(-7).Date);
                break;

            case "Last 30 Days":
                queryTemp = queryTemp.Where(o => o.LastOrder.HasValue && o.LastOrder.Value.Date >= DateTime.Now.AddDays(-30).Date);
                break;

            case "This Month":
                queryTemp = queryTemp.Where(o => o.LastOrder.HasValue && o.LastOrder.Value.Month == DateTime.Now.Month);
                break;

            case "This Year":
                queryTemp = queryTemp.Where(o => o.LastOrder.HasValue && o.LastOrder.Value.Year == DateTime.Now.Year);
                break;

            default:
                break;
        }

        var query = queryTemp;

        if (paramsModel.SortCol == "Name")
        {
            if (paramsModel.SortDr == "asc") query = query.OrderBy(x => x.CustomerName);
            else query = query.OrderByDescending(x => x.CustomerName);
        }
        else if (paramsModel.SortCol == "Date")
        {
            if (paramsModel.SortDr == "asc") query = query.OrderBy(x => x.LastOrder);
            else query = query.OrderByDescending(x => x.LastOrder);
        }
        else if (paramsModel.SortCol == "TotalOrder")
        {
            if (paramsModel.SortDr == "asc") query = query.OrderBy(x => x.TotalOrder);
            else query = query.OrderByDescending(x => x.TotalOrder);
        }
        
        model.Count = query.Count();

        if (paramsModel.Pagenumber < 1) paramsModel.Pagenumber = 1;
        var totalPages = (int)Math.Ceiling((double)model.Count / paramsModel.Pagesize);
        if (paramsModel.Pagenumber > totalPages) paramsModel.Pagenumber = totalPages;
        if (paramsModel.Pagenumber < 1) paramsModel.Pagenumber = 1;


        model.Pagenumber = paramsModel.Pagenumber;
        model.Pagesize = paramsModel.Pagesize;
        model.Searchkey = paramsModel.Searchkey;
        model.SortDr = paramsModel.SortDr;
        model.SortCol = paramsModel.SortCol;
        model.LastDays = paramsModel.LastDays;
        model.CustomerLists = await query.Skip((paramsModel.Pagenumber - 1) * paramsModel.Pagesize).Take(paramsModel.Pagesize).ToListAsync();

        return model;
    }

    public async Task<byte[]> ExportToExcelFile(string LastDays = "All Time", string Searchkey = "", string StartDate = "", string EndDate = "")
    {

         var queryTemp = from customes in _db.Customers
                        where customes.Customername.ToLower().Contains(Searchkey.ToLower())
                        select new CustomerList
                        {
                            CustomerId = customes.Customerid,
                            CustomerName = customes.Customername,
                            CustomerEmail = customes.Customeremail,
                            PhoneNumber = customes.PhoneNumber,
                            LastOrder = (from orders in _db.Orders
                                         where orders.Customerid == customes.Customerid
                                         orderby orders.CreatedDate descending
                                         select orders.CreatedDate).FirstOrDefault(),
                            TotalOrder = (from orders in _db.Orders
                                          where orders.Customerid == customes.Customerid
                                          select orders).Count()
                        };

        DateTime? startDateTime = string.IsNullOrEmpty(StartDate) ? null : DateTime.Parse(StartDate);
        DateTime? endDateTime = string.IsNullOrEmpty(EndDate) ? null : DateTime.Parse(EndDate);

        switch (LastDays)
        {
            case "Custom":
                if (startDateTime != null) queryTemp = queryTemp.Where(o => o.LastOrder.HasValue && o.LastOrder.Value.Date >= startDateTime.Value.Date);
                if (endDateTime != null) queryTemp = queryTemp.Where(o => o.LastOrder.HasValue && o.LastOrder.Value.Date <= endDateTime.Value.Date);
                break;

            case "Last 7 Days":
                queryTemp = queryTemp.Where(o => o.LastOrder.HasValue && o.LastOrder.Value.Date >= DateTime.Now.AddDays(-7).Date);
                break;

            case "Last 30 Days":
                queryTemp = queryTemp.Where(o => o.LastOrder.HasValue && o.LastOrder.Value.Date >= DateTime.Now.AddDays(-30).Date);
                break;

            case "This Month":
                queryTemp = queryTemp.Where(o => o.LastOrder.HasValue && o.LastOrder.Value.Month == DateTime.Now.Month);
                break;

            case "This Year":
                queryTemp = queryTemp.Where(o => o.LastOrder.HasValue && o.LastOrder.Value.Year == DateTime.Now.Year);
                break;

            default:
                break;
        }

        var customerList = await queryTemp.ToListAsync();
        CustomerPage model = new CustomerPage
        {
            CustomerLists = await queryTemp.ToListAsync(),
            Searchkey = Searchkey,
            LastDays = LastDays,
            StartDate = startDateTime ?? null,
            EndDate = endDateTime ?? null,
            Count = await queryTemp.CountAsync()
        };
        if (customerList.Count == 0)
        {
            return Array.Empty<byte>();
        }
        var excel = new Helper.CreateExcel(_db);
        return await excel.CreateExcelFileForCustomer(model);

    }
    public CustomerDetailsList GetCustomerDetails(int customerId)
    {
        CustomerDetailsList model = new CustomerDetailsList();

        Customer? customer = _db.Customers.FirstOrDefault(e => e.Customerid == customerId);
        model.CustomerId = customerId;
        model.CustomerName = customer?.Customername;
        model.PhoneNumber = customer?.PhoneNumber;
        model.ComingSince = customer?.CreatedDate;

        model.Orders = (from orders in _db.Orders
                        where orders.Customerid == customerId
                        orderby orders.Orderid
                        select new CustomerOrders
                        {
                            OrderDate = orders.CreatedDate.HasValue ? DateOnly.FromDateTime(orders.CreatedDate.Value.Date) : default,
                            PaymentType = _db.Payments.FirstOrDefault(x => x.Paymentid == orders.Paymentid).Paymentmode,
                            TotalItem = (from orderitems in _db.Dishes
                                        where orderitems.Orderid == orders.Orderid
                                        select orderitems.Orderid).Count(),
                            TotalAmount = orders.Totalamount,
                            OrderType = (from orderTable in _db.MapOrderTables
                                        where orderTable.Orderid == orders.Orderid
                                        select orderTable.Tablesid).Count() == 0  ? "Take Away" : "Dine In"
                        }).ToList();

        model.Visits = model.Orders.Count;
        model.AvgBill = model.Orders.Average(e=>e.TotalAmount);
        model.MaxBill = model.Orders.Max(e=>e.TotalAmount);
        return model;
    }
}
