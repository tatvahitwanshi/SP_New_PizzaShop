using System.Drawing;
using BusinessLayer.Interface;
using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace BusinessLayer.Repository;

public class OrdersRepository : IOrders
{
    private readonly PizzaShopContext _db;
    public OrdersRepository(PizzaShopContext db)
    {
        _db = db;

    }
    public OrderPage GetOrders(OrdersPaginationParams paramsModel)
    {
        OrderPage orders = new OrderPage();
        var query = from o in _db.Orders
                    join c in _db.Customers on o.Customerid equals c.Customerid
                    join os in _db.Orderstatuses on o.Orderstatusid equals os.Orderstatusid
                    join ps in _db.Payments on o.Paymentid equals ps.Paymentid
                    where o.Orderid.ToString().ToLower().Contains(paramsModel.Searchkey.ToLower()) ||
                          c.Customername.ToLower().Contains(paramsModel.Searchkey.ToLower()) ||
                          os.Statusname.ToLower().Contains(paramsModel.Searchkey.ToLower()) ||
                          ps.Paymentmode.ToLower().Contains(paramsModel.Searchkey.ToLower())
                    orderby o.Orderid
                    select new
                    {
                        order = o,
                        customer = c,
                        orderstatus = os,
                        paymentStatus = ps
                    };
        if (paramsModel.OrderStatusId != 0) query = query.Where(o => o.order.Orderstatusid == paramsModel.OrderStatusId);
        switch (paramsModel.lastDays)
        {
            case "All  Time":
                break;

            case "Last 7 Days":
                query = query.Where(o => o.order.CreatedDate.HasValue &&
                                         o.order.CreatedDate.Value.Date >= DateTime.Now.AddDays(-7).Date);
                break;

            case "Last 30 Days":
                query = query.Where(o => o.order.CreatedDate.HasValue &&
                                         o.order.CreatedDate.Value.Date >= DateTime.Now.AddDays(-30).Date);
                break;
            case "This Month":
                query = query.Where(o => o.order.CreatedDate.HasValue &&
                                         o.order.CreatedDate.Value.Month == DateTime.Now.Month);
                break;
            case "This Year":
                query = query.Where(o => o.order.CreatedDate.HasValue &&
                                       o.order.CreatedDate.Value.Year == DateTime.Now.Year);
                break;

            default:
                break;
        }
        if(paramsModel.startDateTime != null) query = query.Where(o => o.order.CreatedDate.HasValue && o.order.CreatedDate.Value.Date >= paramsModel.startDateTime.Value.Date);
        if(paramsModel.endDateTime != null) query = query.Where(o => o.order.CreatedDate.HasValue && o.order.CreatedDate.Value.Date <= paramsModel.endDateTime.Value.Date);
        
        // model.Taxes = (from Taxes in _db.Taxes
        //     where Taxes.Isenabled == true && Taxes.Isdefault == true && Taxes.Isdeleted != true
        //     select new TaxDetails
        //     {
        //         Taxname = Taxes.Taxname,
        //         Taxvalue = (decimal)Taxes.Taxvalue!,
        //         Taxvaluetype = Taxes.Taxtype,
        //         AppliedTax = (Taxes.Taxtype == "Percentage")
        //                         ? Math.Round(model.SubTotal * Convert.ToDecimal(Taxes.Taxvalue) / 100, 2)
        //                         : Convert.ToDecimal(Taxes.Taxvalue)
        //     }).ToList();

        List<Taxis> taxlist= _db.Taxes.Where(t=> t.Isdefault == true && t.Isenabled == true && t.Isdeleted != true).ToList();


        var mainquery = query.ToList().Select(o =>
        {
            decimal newSubbtotal= o.order.Totalamount ?? 0;
            if(o.orderstatus.Statusname != "Completed")
            {
                decimal taxTotal = 0;

                foreach (var tax in taxlist)
                {
                     taxTotal += (tax.Taxtype == "Percentage")
                                ? Math.Round(newSubbtotal * Convert.ToDecimal(tax.Taxvalue) / 100, 2)
                                 : Convert.ToDecimal(tax.Taxvalue);
                }

                newSubbtotal += taxTotal;

            }
        
            return new OrderList
            {
                Orderid = o.order.Orderid,
                CreatedDate = o.order.CreatedDate,
                CustomerName = o.customer.Customername,
                Status = o.orderstatus.Statusname,
                PaymentMode = o.paymentStatus.Paymentmode,
                Rating = o.order.Rating,
                TotalAmount = newSubbtotal
            };
        }).OrderBy(o => o.Orderid);



        if (paramsModel.sortCol == "OrderNo")
        {
            if (paramsModel.sortDr == "asc") mainquery = mainquery.OrderBy(x => x.CreatedDate);
            else mainquery = mainquery.OrderByDescending(x => x.CreatedDate);
        }
        else if (paramsModel.sortCol == "OrderDate")
        {
            if (paramsModel.sortDr == "asc") mainquery = mainquery.OrderBy(x => x.CreatedDate);
            else mainquery = mainquery.OrderByDescending(x => x.CreatedDate);
        }
        else if (paramsModel.sortCol == "CustomerName")
        {
            if (paramsModel.sortDr == "asc") mainquery = mainquery.OrderBy(x => x.CustomerName);
            else mainquery = mainquery.OrderByDescending(x => x.CustomerName);
        }
        else if (paramsModel.sortCol == "TotalAmount")
        {
            if (paramsModel.sortDr == "asc") mainquery = mainquery.OrderBy(x => x.TotalAmount);
            else mainquery = mainquery.OrderByDescending(x => x.TotalAmount);
        }
        orders.Count = mainquery.Count();
        if (paramsModel.Pagenumber < 1) paramsModel.Pagenumber  = 1;
        var totalPages = (int)Math.Ceiling((double)orders.Count / paramsModel.Pagesize);
        if (paramsModel.Pagenumber  > totalPages) paramsModel.Pagenumber  = totalPages;
        if (paramsModel.Pagenumber  < 1) paramsModel.Pagenumber  = 1;

        orders.OrderStatusId = paramsModel.OrderStatusId;
        orders.Pagenumber = paramsModel.Pagenumber;
        orders.Pagesize = paramsModel.Pagesize;
        orders.Searchkey = paramsModel.Searchkey;
        orders.sortDr = paramsModel.sortDr;
        orders.sortCol = paramsModel.sortCol;
        orders.lastDays = paramsModel.lastDays;
        orders.startDate = paramsModel.startDateTime;
        orders.endDate = paramsModel.endDateTime;
        orders.OrderTableLists = mainquery.Skip((paramsModel.Pagenumber - 1) * paramsModel.Pagesize).Take(paramsModel.Pagesize).ToList();

        return orders;
    }

    public List<Orderstatus> GetOrderStatus()
    {
        return (from os in _db.Orderstatuses
                orderby os.Statusname
                select new Orderstatus
                {
                    Orderstatusid = os.Orderstatusid,
                    Statusname = os.Statusname,
                }).ToList();

    }

    public async Task<byte[]> ExportToExcel(int orderStatusId = 0, string lastDays = "All Time", string searchKey = "")
    {
        OrderPage orders = new OrderPage();
        var query = from o in _db.Orders
                    join c in _db.Customers on o.Customerid equals c.Customerid
                    join os in _db.Orderstatuses on o.Orderstatusid equals os.Orderstatusid
                    join ps in _db.Payments on o.Paymentid equals ps.Paymentid
                    where o.Orderid.ToString().ToLower().Contains(searchKey.ToLower()) ||
                          c.Customername.ToLower().Contains(searchKey.ToLower()) ||
                          os.Statusname.ToLower().Contains(searchKey.ToLower()) ||
                          ps.Paymentmode.ToLower().Contains(searchKey.ToLower())
                    orderby o.Orderid
                    select new
                    {
                        order = o,
                        customer = c,
                        orderstatus = os,
                        paymentStatus = ps
                    };


        if (orderStatusId != 0) query = query.Where(o => o.order.Orderstatusid == orderStatusId);
        switch (lastDays)
        {
            case "All  Time":
                break;

            case "Last 7 Days":
                query = query.Where(o => o.order.CreatedDate.HasValue &&
                                         o.order.CreatedDate.Value.Date >= DateTime.Now.AddDays(-7).Date);
                break;

            case "Last 30 Days":
                query = query.Where(o => o.order.CreatedDate.HasValue &&
                                         o.order.CreatedDate.Value.Date >= DateTime.Now.AddDays(-30).Date);
                break;
            case "This Month":
                query = query.Where(o => o.order.CreatedDate.HasValue &&
                                         o.order.CreatedDate.Value.Month == DateTime.Now.Month);
                break;
            case "This Year":
                query = query.Where(o => o.order.CreatedDate.HasValue &&
                                       o.order.CreatedDate.Value.Year == DateTime.Now.Year);
                break;

            default:
                break;
        }
        int totalCount = query.Count();

        List<Taxis> taxlist= _db.Taxes.Where(t=> t.Isdefault == true && t.Isenabled == true && t.Isdeleted != true).ToList();

        var data = await query.ToListAsync();

        // Step 2: Transform data in-memory
        List<OrderList> list = data.Select(o =>
        {
            decimal newSubbtotal = o.order.Totalamount ?? 0;
            if (o.orderstatus.Statusname != "Completed")
            {
                decimal taxTotal = 0;

                foreach (var tax in taxlist)
                {
                    taxTotal += (tax.Taxtype == "Percentage")
                                ? Math.Round(newSubbtotal * Convert.ToDecimal(tax.Taxvalue) / 100, 2)
                                : Convert.ToDecimal(tax.Taxvalue);
                }

                newSubbtotal += taxTotal;
            }

            return new OrderList
            {
                Orderid = o.order.Orderid,
                CreatedDate = o.order.CreatedDate,
                CustomerName = o.customer.Customername,
                Status = o.orderstatus.Statusname,
                PaymentMode = o.paymentStatus.Paymentmode,
                Rating = o.order.Rating ?? 0,
                TotalAmount = newSubbtotal
            };
        }).OrderBy(o => o.Orderid).ToList();

        if (list.Count == 0)
        {
            return Array.Empty<byte>();
        }
        var createExcelHelper = new Helper.CreateExcel(_db);
        return await createExcelHelper.CreateExcelFile(list, searchKey, lastDays, orderStatusId, totalCount);

    }


    public async Task<OrderDetailsViewModel> getOrderDetails(int orderId = 1)
    {
        OrderDetailsViewModel model = new OrderDetailsViewModel();

        // basic details 
        model.OrderId = orderId;
        model.OrderStatus = _db.Orderstatuses.Where(o => o.Orderstatusid == _db.Orders.Where(o => o.Orderid == orderId).Select(o => o.Orderstatusid).FirstOrDefault()).Select(o => o.Statusname).FirstOrDefault();
        model.PaymentStatus = _db.Payments.Where(p => p.Paymentid == _db.Orders.Where(o => o.Orderid == orderId).Select(o => o.Paymentid).FirstOrDefault()).Select(p => p.Paymentmode).FirstOrDefault();
        model.SubTotal = 0;
        Order? obj = await _db.Orders.FindAsync(orderId);

        Invoice? invoice = await _db.Invoices.Where(e => e.Orderid == orderId).FirstOrDefaultAsync();
        if (invoice != null)
        {
            model.InvoiceId = invoice.Invoiceid;
            model.PaidOn = invoice?.Paidon ?? default;

            // Generate unique Invoice Number if not already set
            if (string.IsNullOrEmpty(invoice.Invoicenumber))
            {
                DateTime createdDate = obj.CreatedDate ?? DateTime.Now;

                // Format: #DOMYYYYMMDDHHMMSSInvoiceID
                string datePart = createdDate.ToString("yyyyMMddHHmmss");
                string uniqueInvoiceNumber = $"#DOM{datePart}{invoice.Invoiceid}";                
                invoice.Invoicenumber = uniqueInvoiceNumber;
                model.InvoiceNumber = uniqueInvoiceNumber; // Pass to ViewModel
            }
            else
            {
                model.InvoiceNumber = invoice.Invoicenumber;
            }
        }

        if (obj != null)
        {
            model.CreatedDate = obj.CreatedDate;
            model.CompletedTime = obj?.Completedtime ?? DateTime.MinValue;
            model.Personcount = (short)(obj?.Personcount ?? 0);
        }

        // Customer details 
        Customer? customer = await _db.Customers.FindAsync(obj?.Customerid);
        if (customer != null)
        {
            model.Customer = new CustomerDetails
            {
                Name = customer.Customername,
                Email = customer.Customeremail,
                Phone = customer.PhoneNumber
            };
        }

        // Table details
        List<int?> tableIds = _db.MapOrderTables.Where(o => o.Orderid == orderId).Select(o => (int?)o.Tablesid).ToList();
        model.Tables = new TableDetails();
        model.Tables.TableList = new List<string?>();

        for (int i = 0; i < tableIds.Count; i++)
        {
            model.Tables.TableList.Add(_db.Tables.Where(t => t.Tablesid == tableIds[i]).Select(t => t.Tablename).FirstOrDefault());
            model.Tables.SectionName = _db.Sections.Where(s => s.Sectionid == _db.Tables.Where(t => t.Tablesid == tableIds[i]).Select(t => t.Sectionid).FirstOrDefault()).Select(s => s.Sectionname).FirstOrDefault();
        }

        // Dish details
        model.Dishes = (from Dishes in _db.Dishes
                        where Dishes.Orderid == orderId
                        select new DishDetails
                        {
                            Itemname = Dishes.Itemname,
                            Quantity = Dishes.Quantity,
                            Price = Dishes.Price,
                            Total = Dishes.Quantity * Dishes.Price,
                            modifiers = (from dishModifier in _db.Dishmodifiers
                                         where dishModifier.Dishid == Dishes.Dishid
                                         select new ModifierDetails
                                         {
                                             Modifiername = dishModifier.Modifiername,
                                             Quantity = dishModifier.Quantity,
                                             Price = dishModifier.Price,
                                             Total = dishModifier.Quantity * dishModifier.Price * Dishes.Quantity
                                         }).ToList()
                        }).ToList();

        foreach (var dish in model.Dishes)
        {
            model.SubTotal += dish.Total;
            foreach (var modifier in dish.modifiers)
            {
                model.SubTotal += modifier.Total;
            }
        }

        model.Total = model.SubTotal;

        // Taxes details
        model.Taxes = (from InvoiceTax in _db.Invoicetaxes
                       where InvoiceTax.Invoiceid == model.InvoiceId
                       select new TaxDetails
                       {
                           Taxname = InvoiceTax.Taxname,
                           Taxvalue = InvoiceTax.Taxvalue,
                           Taxvaluetype = InvoiceTax.Taxvaluetype,
                           AppliedTax = (InvoiceTax.Taxvaluetype == "Percentage")
                                         ? Math.Round(model.SubTotal * InvoiceTax.Taxvalue / 100, 2)
                                         : Convert.ToDecimal(InvoiceTax.Taxvalue)
                       }).ToList();
        
        if((model.Taxes == null || model.Taxes!.Count == 0 ) )
        {
            model.Taxes = (from Taxes in _db.Taxes
            where Taxes.Isenabled == true && Taxes.Isdefault == true && Taxes.Isdeleted != true
            select new TaxDetails
            {
                Taxname = Taxes.Taxname,
                Taxvalue = (decimal)Taxes.Taxvalue!,
                Taxvaluetype = Taxes.Taxtype,
                AppliedTax = (Taxes.Taxtype == "Percentage")
                                ? Math.Round(model.SubTotal * Convert.ToDecimal(Taxes.Taxvalue) / 100, 2)
                                : Convert.ToDecimal(Taxes.Taxvalue)
            }).ToList();
        }

        foreach (var tax in model.Taxes)
        {
            model.Total += tax.AppliedTax;
        }

        // Order objc = await _db.Orders.FindAsync(orderId);
        // objc.Totalamount = model.Total;

        // _db.SaveChanges();

        return model;
    }

    public async Task<byte[]> ExportToPdf(int orderId = 1)
    {
        OrderDetailsViewModel model = await getOrderDetails(orderId);
        var pdf = new Helper.CreatePdf(_db);
        return pdf.CreatePdfFile(model);
    }


}
