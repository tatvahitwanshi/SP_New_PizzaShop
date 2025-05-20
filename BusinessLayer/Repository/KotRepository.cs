using System.Data;
using System.Data.Common;
using System.Diagnostics;
using BusinessLayer.Interface;
using Dapper;
using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace BusinessLayer.Repository;

public class KotRepository : IKot
{
    private readonly PizzaShopContext _db;

    public KotRepository(PizzaShopContext db)
    {
        _db = db;
    }
    public async Task<List<CategoryList>> GetCategoryLists()
    {
        // return await (from e in _db.Categories
        //               where e.Isdeleted == false
        //               orderby e.Categoryid
        //               select new CategoryList
        //               {
        //                   CategoryId = e.Categoryid,
        //                   CategoryName = e.Categoryname
        //               }).ToListAsync();
        return await _db.Set<Category>()
            .FromSqlRaw("SELECT * FROM sp_get_categories()")
            .Select(e => new CategoryList
            {
                CategoryId = e.Categoryid,
                CategoryName = e.Categoryname
            })
            .ToListAsync();
    }

    public async Task<string> GetCategoryName(int id)
    {
        if (id == 0) return "All";

        Category? category = await _db.Categories.Where(e => e.Categoryid == id).FirstOrDefaultAsync();
        return category?.Categoryname ?? string.Empty;
    }

    // public List<OrderCards> GetOrderCards(int CurrentCategory, string orderStatus)
    // {

    //     List<OrderCards> result = new List<OrderCards>();
    //     HashSet<int> matchedOrderIds = new HashSet<int>();

    //     //Collect all matching order IDs based on category and status
    //     matchedOrderIds = (from ord in _db.Orders
    //                            join dsh in _db.Dishes on ord.Orderid equals dsh.Orderid
    //                            join itm in _db.Items on new { dsh.Itemid ,dsh.Itemname} equals new {itm.Itemid,itm.Itemname} 
    //                            join status in _db.Orderstatuses on ord.Orderstatusid equals status.Orderstatusid
    //                            where (CurrentCategory == 0 || itm.Categoryid == CurrentCategory) && status.Statusname=="In Progress" &&
    //                                  ((orderStatus == "In Progress" && dsh.Inprogressquantity > 0) ||
    //                                   (orderStatus == "Ready" && dsh.Readyquantity > 0))
    //                            select ord.Orderid).Distinct().ToHashSet();

    //     // Generate final list of OrderCards
    //     result = (from ord in _db.Orders
    //               where matchedOrderIds.Contains(ord.Orderid)
    //               select new OrderCards
    //               {
    //                   OrderId = ord.Orderid,
    //                   OrderTime = ord.CreatedDate,
    //                   OrderInstructions = ord.Instruction,

    //                   TableNameList = (from tbl in _db.Tables
    //                                    join mapTbl in _db.MapOrderTables on tbl.Tablesid equals mapTbl.Tablesid
    //                                    where mapTbl.Orderid == ord.Orderid
    //                                    select tbl.Tablename).ToList(),

    //                   SectionName = (from tbl in _db.Tables
    //                                  join sec in _db.Sections on tbl.Sectionid equals sec.Sectionid
    //                                  join mapTbl in _db.MapOrderTables on tbl.Tablesid equals mapTbl.Tablesid
    //                                  where mapTbl.Orderid == ord.Orderid
    //                                  select sec.Sectionname).FirstOrDefault(),


    //                   ItemList = (from dsh in _db.Dishes
    //                               join itm in _db.Items on dsh.Itemid equals itm.Itemid
    //                               where dsh.Orderid == ord.Orderid &&
    //                                     (CurrentCategory == 0 || itm.Categoryid == CurrentCategory) &&
    //                                     ((orderStatus == "In Progress" && dsh.Inprogressquantity > 0) ||
    //                                      (orderStatus == "Ready" && dsh.Readyquantity > 0))
    //                               select new SingleItem
    //                               {
    //                                   DishId = dsh.Dishid,
    //                                   ItemId = dsh.Itemid,
    //                                   ItemName = dsh.Itemname,
    //                                   inProgressQuantity = dsh.Inprogressquantity,
    //                                   pendingQuantity = dsh.Pendingquantity,
    //                                   TotalQuantity = dsh.Quantity,
    //                                   ItemInstructions = dsh.Iteminstruction,
    //                                   Quantity = orderStatus == "In Progress" ? dsh.Inprogressquantity :
    //                                              orderStatus == "Ready" ? dsh.Readyquantity : dsh.Quantity,
    //                                   ModifiersList = (from mod in _db.Dishmodifiers
    //                                                    where mod.Dishid == dsh.Dishid
    //                                                    select new SingleModifier
    //                                                    {
    //                                                        ModifierId = mod.Modifierid,
    //                                                        ModifierName = mod.Modifiername
    //                                                    }).ToList()
    //                               }).ToList()
    //               }).ToList();

    //     return result;
    // }
    public List<OrderCards> GetOrderCards(int CurrentCategory, string orderStatus)
    {
        List<OrderCards> result = new List<OrderCards>();

        var connection = _db.Database.GetDbConnection();

        var matchedOrderIds = connection.Query<int>(
            "SELECT * FROM get_matched_order_ids(@current_category, @order_status);",
            new { current_category = CurrentCategory, order_status = orderStatus }
        ).ToList();


        foreach (int orderid in matchedOrderIds)
        {
            OrderCards oneOrder = new OrderCards();
            Order? ord = _db.Orders.Where(o => o.Orderid == orderid).FirstOrDefault();
            oneOrder.OrderId = ord!.Orderid;
            oneOrder.OrderTime = ord.CreatedDate;
            oneOrder.OrderInstructions = ord.Instruction;
            (string sectionName, List<string> tableList) = GetSectionAndTables(orderid, connection);
            oneOrder.SectionName = sectionName;
            oneOrder.TableNameList = tableList;
            oneOrder.ItemList = GetItemList(orderid, CurrentCategory, orderStatus, connection);
            result.Add(oneOrder);
        }

        return result;
    }
    public (string sectionName, List<string> tableList) GetSectionAndTables(int orderId, DbConnection conn)
    {

        var result = conn.Query<(string SectionName, string TableName)>(
            "SELECT * FROM get_section_and_tables(@order_id);",
            new { order_id = orderId }
        ).ToList();

        var sectionName = result.FirstOrDefault().SectionName ?? string.Empty;
        var tableList = result.Select(r => r.TableName).ToList();

        return (sectionName, tableList);
    }
    public List<SingleItem> GetItemList(int orderId, int currentCategory, string orderStatus, DbConnection conn)
    {
        var items = conn.Query<SingleItem>(
            "SELECT * FROM get_item_list(@order_id, @category, @status);",
            new { order_id = orderId, category = currentCategory, status = orderStatus }
        ).ToList();

        foreach (var item in items)
        {
            item.ModifiersList = conn.Query<SingleModifier>(
                "SELECT * FROM get_dish_modifiers(@dish_id);",
                new { dish_id = item.DishId }
            ).ToList();
        }

        return items;
    }

    public async Task UpdateChangeQuantity(OrderCards ordercard, string OrderStatus)
    {
        if (ordercard.ItemList != null)
        {
            foreach (var item in ordercard.ItemList)
            {
                if (item.IsSelected)
                {
                    await _db.Database.ExecuteSqlRawAsync(
                        "CALL sp_update_dish_quantity({0}, {1}, {2})",
                        item.DishId!,
                        item.Quantity!,
                        OrderStatus
                    );
                }
                // Dish? dish = await _db.Dishes.Where(e => e.Dishid == item.DishId).FirstOrDefaultAsync();
                // if (dish != null && item.IsSelected)
                // {
                //     if (OrderStatus == "In Progress")
                //     {
                //         dish.Inprogressquantity = (short?)(dish.Inprogressquantity + item.Quantity);
                //         dish.Readyquantity = (short?)(dish.Readyquantity - item.Quantity);
                //     }
                //     else if (OrderStatus == "Ready")
                //     {
                //         dish.Readyquantity = (short?)(dish.Readyquantity + item.Quantity);
                //         dish.Inprogressquantity = (short?)(dish.Inprogressquantity - item.Quantity);
                //     }
                //     _db.Dishes.Update(dish);
                // }
            }
            // await _db.SaveChangesAsync();
        }

    }
    public async Task<List<PendingOrders>> GetPendingOrders()
    {
        // return await _db.Orders
        //              .Where(e => e.Orderstatusid == _db.Orderstatuses
        //                                             .FirstOrDefault(p=>p.Statusname == "Pending")!
        //                                             .Orderstatusid)
        //              .OrderBy(e=>e.Orderid)
        //              .Select(e=> new PendingOrders{
        //                 OrderID = e.Orderid,
        //                 OrderTime = (DateTime)e.CreatedDate!,
        //                 Amount = e.Totalamount
        //             }).ToListAsync();
        var conn = _db.Database.GetDbConnection();
        var result = await conn.QueryAsync<PendingOrders>("SELECT * FROM sp_get_pending_orders()");
        return result.ToList();
    }

    public async Task SetToInProgress(List<int> model)
    {
        int? inProgressStatusId = _db.Orderstatuses
            .FirstOrDefault(e => e.Statusname == "In Progress")?.Orderstatusid;

        if (inProgressStatusId == null)
            throw new Exception("In Progress status not found.");

        foreach (int orderId in model)
        {
            await _db.Database.ExecuteSqlRawAsync(
                "CALL sp_set_order_inprogress({0}, {1})",
                orderId,
                inProgressStatusId
            );
            // var order = await _db.Orders.FirstOrDefaultAsync(e => e.Orderid == orderId);
            // if (order == null)
            //     continue;

            // order.Orderstatusid = (int)inProgressStatusId;

            // var dishes = await _db.Dishes.Where(e => e.Orderid == orderId).ToListAsync();
            // foreach (var dish in dishes)
            // {
            //     dish.Inprogressquantity = dish.Quantity;
            //     dish.Pendingquantity = 0;
            // }
            // await _db.SaveChangesAsync();
        }
    }

    public bool IsServed(int orderId)
    {

        return !_db.Dishes.Any(e => e.Orderid == orderId && (e.Pendingquantity + e.Inprogressquantity) > 0);
    }

    public async Task<(bool, string)> MarkOrderServed(int orderId)
    {
        try
        {
            using (var conn = _db.Database.GetDbConnection())
            {
                await conn.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("p_order_id", orderId);
                parameters.Add("success", dbType: DbType.Boolean, direction: ParameterDirection.Output);
                parameters.Add("message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                await conn.ExecuteAsync("mark_order_served", parameters, commandType: CommandType.StoredProcedure);

                bool isSuccess = parameters.Get<bool>("success");
                string resultMessage = parameters.Get<string>("message");

                return (isSuccess, resultMessage);
            }
        }
        catch
        {
            throw;
        }

    }


}
