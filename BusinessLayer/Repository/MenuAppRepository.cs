using System.Data;
using BusinessLayer.Interface;
using Dapper;
using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using Org.BouncyCastle.Pqc.Crypto.Frodo;

namespace BusinessLayer.Repository;

public class MenuAppRepository : IMenuApp
{
    private readonly PizzaShopContext _db;
    private readonly IMenu _menu;

    private readonly IWaitingList _waitingList;
    public MenuAppRepository(PizzaShopContext db, IMenu menu, IWaitingList waitingList)
    {
        _db = db;
        _menu = menu;
        _waitingList = waitingList;
    }

    public async Task<List<CategoryList>> GetCategoryLists()
    {
        // return await _db.Categories
        //             .Where(x => x.Isdeleted != true)
        //             .OrderBy(x => x.Categoryid)
        //             .Select(x => new CategoryList
        //             {
        //                 CategoryId = x.Categoryid,
        //                 CategoryName = x.Categoryname
        //             }).ToListAsync();
        return await _db.Set<Category>()
            .FromSqlRaw("SELECT * FROM sp_get_categories()")
            .Select(e => new CategoryList
            {
                CategoryId = e.Categoryid,
                CategoryName = e.Categoryname
            })
            .ToListAsync();
    }

    public async Task<int> checkTableValue(int tableId)
    {
        Table? table = await _db.Tables.FirstOrDefaultAsync(e => e.Tablesid == tableId);
        if (table == null || table.Isoccupied == false)
        {
            return -1;
        }
        return tableId;
    }

    public async Task<List<TaxesView>> GetTaxList()
    {
        // return await (from taxes in _db.Taxes
        //               where taxes.Isdeleted == false && taxes.Isenabled == true
        //               orderby taxes.Isdefault descending
        //               select new TaxesView
        //               {
        //                   TaxesId = taxes.Taxesid,
        //                   Taxname = taxes.Taxname,
        //                   Taxvalue = taxes.Taxvalue,
        //                   Taxtype = taxes.Taxtype,
        //                   Isdefault = (bool)taxes.Isdefault!
        //               }).ToListAsync();
        return await _db.Set<Taxis>()
            .FromSqlRaw("SELECT * FROM get_active_taxes()")
            .Select(e => new TaxesView
            {
                TaxesId = e.Taxesid,
                Taxname = e.Taxname,
                Taxvalue = e.Taxvalue,
                Taxtype = e.Taxtype,
                Isdefault = e.Isdefault
            })
            .ToListAsync();
    }

    public async Task<List<SingleItemForOrderApp>> getItemList(int categoryId, string SearchKey)
    {
        // List<SingleItemForOrderApp> list;
        // list = await (from item in _db.Items
        //               where item.Isdeleted == false && item.Isavailable == true
        //                     && (categoryId == 0 || (categoryId == -1 && item.Isfavourite == true) || item.Categoryid == categoryId)
        //                     && item.Itemname.ToLower().Contains(SearchKey.ToLower())
        //               orderby item.Itemid
        //               select new SingleItemForOrderApp
        //               {
        //                   ItemId = item.Itemid,
        //                   ItemName = item.Itemname,
        //                   ItemType = item.Itemtype,
        //                   Rate = (int)item.Rate!,
        //                   ImageUrl = item.Itemimage,
        //                   isFavourite = item.Isfavourite,
        //                   isModifier = _db.MapItemsModifiersgroups.Any(e => e.Itemid == item.Itemid),
        //               }).ToListAsync();

        // return list;
        using var connection = _db.Database.GetDbConnection();
        var result = await connection.QueryAsync<SingleItemForOrderApp>(
            "SELECT * FROM get_item_list(@p_category_id, @p_search_key)",
            new { p_category_id = categoryId, p_search_key = SearchKey }
        );
        return result.ToList();
    }

    public async Task<bool> MarkFavouriteAsync(int itemId, bool isFavourite)
    {
        Item? item = await _db.Items.FirstOrDefaultAsync(e => e.Itemid == itemId);

        if (item == null) return false;

        item.Isfavourite = isFavourite;
        await _db.SaveChangesAsync();

        return true;
    }
    public async Task<ModifierDetailsForItem> getModifierItemDetails(int itemId)
    {
        using var connection = _db.Database.GetDbConnection();
        var json = await connection.ExecuteScalarAsync<string>(
        "SELECT get_modifier_item_details(@item_id)", new { item_id = itemId });

        return JsonConvert.DeserializeObject<ModifierDetailsForItem>(json!)!;
        // ModifierDetailsForItem model = new ModifierDetailsForItem();
        // Item? item = await _db.Items.FirstOrDefaultAsync(e => e.Itemid == itemId);

        // if (item == null) return model;

        // model.ItemId = item.Itemid;
        // model.ItemName = item.Itemname;
        // model.ItemType = item.Itemtype;
        // model.Price = item.Rate;

        // model.ModGroupList = await (from itemModGroupMap in _db.MapItemsModifiersgroups
        //                             where itemModGroupMap.Itemid == itemId
        //                             select new ModifierGroupDetailsOrderApp
        //                             {
        //                                 ModGroupId = itemModGroupMap.Modifiersgroupid,
        //                                 Min = itemModGroupMap.Minimum,
        //                                 Max = itemModGroupMap.Maximum,
        //                                 ModGroupName = _db.Modifiersgroups.FirstOrDefault(e => e.Modifiersgroupid == itemModGroupMap.Modifiersgroupid)!.Modifiersgroupname,
        //                                 ModifierList = (from modItemModGroupMap in _db.MapModifiersgroupModifiers
        //                                                 join modItem in _db.Modifiers on modItemModGroupMap.Modifiersid equals modItem.Modifiersid
        //                                                 where modItemModGroupMap.Modifiersgroupid == itemModGroupMap.Modifiersgroupid
        //                                                 select new ModifierItems
        //                                                 {
        //                                                     ModifierId = modItem.Modifiersid,
        //                                                     ModifierName = modItem.Modifiersname,
        //                                                     Price = modItem.Modifiersrate
        //                                                 }).ToList()
        //                             }).ToListAsync();
        // return model;
    }

    public async Task<TokenOrOrderDetails?> getTokenOrderDetails(int tableId)
    {
        using var connection = _db.Database.GetDbConnection();
        var jsonResult = await connection.ExecuteScalarAsync<string>(
            "SELECT get_token_order_details_menuapp(@table_id);",
            new { table_id = tableId }
        );

        return jsonResult != null
            ? JsonConvert.DeserializeObject<TokenOrOrderDetails>(jsonResult)
            : null;
    }


    // public async Task<TokenOrOrderDetails?> getTokenOrderDetails(int tableId)
    // {
    //     TokenOrOrderDetails? model = new TokenOrOrderDetails();
    //     List<int?> tableIds = new List<int?>();
    //     // get table details
    //     Table? table = _db.Tables.FirstOrDefault(e => e.Tablesid == tableId && e.Isdeleted == false);
    //     if (table == null) return model;
    //     if (table.Isoccupied == true && table.Isrunning == true)
    //     {
    //         // order is already there.....
    //         model.TokenORorder = "order";
    //         model.Id = table.Currenttokenid;
    //         model.OrderId = table.Currenttokenid;
    //         Order? order = _db.Orders.FirstOrDefault(e => e.Orderid == table.Currenttokenid);
    //         if(order != null)
    //         {
    //             model.CustomerId = order.Customerid;
    //             model.Instruction = order.Instruction;
    //             model.NumberOfPerson = order.Personcount;
    //         }
    //         tableIds = _db.MapOrderTables.Where(o => o.Orderid == table.Currenttokenid).Select(o => (int?)o.Tablesid).ToList();
    //     }
    //     else if (table.Isoccupied == true && table.Isrunning == false)
    //     {
    //         // token is taking order first time
    //         model.TokenORorder = "token";
    //         model.Id = table.Currenttokenid;
    //         model.TokenId = table.Currenttokenid;
    //         model.CustomerId = _db.WaitingTables.FirstOrDefault(e => e.Waitingid == table.Currenttokenid)!.Customerid;
    //         model.NumberOfPerson = _db.WaitingTables.FirstOrDefault(e => e.Waitingid == table.Currenttokenid)!.Totalperson;
    //         tableIds = _db.MapTableTokens.Where(o => o.Tokenid == table.Currenttokenid).Select(o => (int?)o.Tableid).ToList();
    //     }
    //     // get customer details
    //     model.CustomerDetail = await _db.Customers.Where(e => e.Customerid == model.CustomerId)
    //                            .Select(e => new CustomerDetails
    //                            {
    //                                Email = e.Customeremail,
    //                                Phone = e.PhoneNumber,
    //                                Name = e.Customername
    //                            }).FirstOrDefaultAsync();

    //     // get table details
    //     model.TableDetail = new TableDetails();
    //     model.TableDetail.TableList = new List<string?>();
    //     int MaxTableCapacity=0;

    //     for (int i = 0; i < tableIds.Count; i++)
    //     {
    //         model.TableDetail.TableList.Add(_db.Tables.Where(t => t.Tablesid == tableIds[i]).Select(t => t.Tablename).FirstOrDefault());
    //         model.TableDetail.SectionName = _db.Sections.Where(s => s.Sectionid == _db.Tables.Where(t => t.Tablesid == tableIds[i]).Select(t => t.Sectionid).FirstOrDefault()).Select(s => s.Sectionname).FirstOrDefault();

    //         MaxTableCapacity += int.Parse(_db.Tables.Where(t => t.Tablesid == tableIds[i]).Select(t => t.Tablecapacity).FirstOrDefault() ?? "0");
    //         model.MaxTableCapacity= MaxTableCapacity;
    //     }

    //     // get already placed order list if it is order
    //     if (model.TokenORorder == "order")
    //     {
    //         var dishEntities = await _db.Dishes
    //                                 .Where(e => e.Orderid == model.OrderId)
    //                                 .OrderBy(e => e.Dishid)
    //                                 .ToListAsync();

    //         model.CurrOrder = dishEntities.Select(e =>
    //         {
    //             var modifierList = _db.Dishmodifiers
    //                                   .Where(p => p.Dishid == e.Dishid)
    //                                   .Select(p => new OrderedModifier
    //                                   {
    //                                       ModifierId = p.Modifierid,
    //                                       ModifierName = p.Modifiername,
    //                                       Price = p.Price,
    //                                       Quantity = p.Quantity
    //                                   }).ToList();

    //             return new OrderedItem
    //             {
    //                 Id = e.Dishid,
    //                 ItemId = e.Itemid,
    //                 ItemName = e.Itemname,
    //                 Price = e.Price,
    //                 Quantity = e.Quantity,
    //                 ReadyQuantity = e.Readyquantity,
    //                 ServedQuantity = e.Inservedquantity,
    //                 Instruction = e.Iteminstruction,
    //                 IsEdit = IsEditableItem(e.Dishid),
    //                 IsServed = e.Quantity == e.Inservedquantity,
    //                 ModifierList = modifierList
    //             };
    //         }).ToList();
    //     }
    //     return model;
    // }

    // private bool IsEditableItem(int dishId)
    // {
    //     Dish dish = _db.Dishes.FirstOrDefault(e => e.Dishid == dishId)!;
    //     Order order = _db.Orders.FirstOrDefault(e => e.Orderid == dish.Orderid)!;

    //     String orderStatus = _db.Orderstatuses.FirstOrDefault(e => e.Orderstatusid == order.Orderstatusid)!.Statusname;
    //     if (orderStatus == "Pending")
    //         return true;
    //     else if (orderStatus != "In Progress")
    //         return false;

    //     if (dish.Inprogressquantity > 0)
    //         return true;

    //     return false;
    // }

    public OrderedItem calculateItemValuesPrice(OrderedItem model)
    {
        model.ItemPriceTotal = model.Price;
        model.ModifierPriceTotal = 0;


        if (model.ModifierList != null)
        {
            for (int i = 0; i < model.ModifierList.Count; i++)
            {
                // for (int j = i + 1; j < model.ModifierList.Count; j++)
                // {
                //     // if (model.ModifierList[i].ModifierId == model.ModifierList[j].ModifierId && model.ModifierList[i].ModifierGroup != null && model.ModifierList[j].ModifierGroup != null)
                //     // {
                //     //     model.ModifierList[i].ModifierGroupName = "(" + _db.Modifiersgroups.FirstOrDefault(e => e.Modifiersgroupid == model.ModifierList[i].ModifierGroup)!.Modifiersgroupname.Split(" ")[0] + ")";
                //     //     model.ModifierList[j].ModifierGroupName = "(" + _db.Modifiersgroups.FirstOrDefault(e => e.Modifiersgroupid == model.ModifierList[j].ModifierGroup)!.Modifiersgroupname.Split(" ")[0] + ")";

                //     // }
                // }
                if (model.ModifierList[i].Quantity == null)
                {
                    model.ModifierList[i].Quantity = 1;
                }
                model.ModifierPriceTotal += model.ModifierList[i].Quantity * model.ModifierList[i].Price;
                model.ModifierList[i].ModifierName += " x" + model.ModifierList[i].Quantity;
            }
        }
        return model;
    }

    public async Task<string> SavePlaceOrder(TokenOrOrderDetails model)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            string message = "";
            // create new order--------------------------
            if (model.TokenORorder == "token") model.OrderId = await CreateOrder(model);
            else model.OrderId = model.Id;

            // get already placed order list if it is order------------------------
            Order order = _db.Orders.FirstOrDefault(e => e.Orderid == model.OrderId)!;
            if (order == null)
            {
                message = "Order not found";
                return message;
            }
            string orderStatus = _db.Orderstatuses.FirstOrDefault(e => e.Orderstatusid == order.Orderstatusid)!.Statusname;
            if (orderStatus == "Completed" || orderStatus == "Cancellled")
            {
                message = "Order status is completed or cancelled";
                return message;
            }

            order.Instruction = model.Instruction;
            order.Personcount = model.NumberOfPerson;

            if (model.TokenORorder == "order")
            {
                order.EditDate = DateTime.Now;
                order.EditedBy = model.By;
            }
            _db.SaveChanges();

            //get order status-------------------------------------------------
            if (model.CurrOrder == null)
            {
                message = "Order not found";
                return message;
            }


            // get the already placed order list---------------------------------
            List<int> preOrderedIds = _db.Dishes
                                        .Where(e => e.Orderid == model.OrderId)
                                        .Select(e => e.Dishid)
                                        .ToList();
            List<int?> currentOrderIds = model.CurrOrder.Select(e => e.Id).ToList();

            if (orderStatus == "Pending" || orderStatus == "In Progress")
            {
                // delete the items which are not in the current order
                List<int> deleteIds = preOrderedIds
                            .Where(id =>
                            {
                                var dish = _db.Dishes.FirstOrDefault(d => d.Dishid == id);
                                if (dish == null)
                                    return false;

                                bool isFullyUnprepared = (dish.Pendingquantity + dish.Inprogressquantity) == dish.Quantity;

                                return !currentOrderIds.Contains(id) && isFullyUnprepared;
                            })
                            .ToList();
                if (deleteIds.Any())
                {
                    List<Dishmodifier> dishmodifiersToDelete = _db.Dishmodifiers
                        .Where(e => deleteIds.Contains(e.Dishid))
                        .ToList();
                    List<Dish> dishesToDelete = _db.Dishes
                        .Where(e => deleteIds.Contains(e.Dishid))
                        .ToList();
                    _db.Dishmodifiers.RemoveRange(dishmodifiersToDelete);
                    _db.Dishes.RemoveRange(dishesToDelete);
                    await _db.SaveChangesAsync();
                }

            }

            //update quantity and item instrution of the already placed order--------------------------
            List<int> updateIds = currentOrderIds
                            .Where(id =>
                            {
                                if (id == null)
                                    return false;
                                Dish? dish = _db.Dishes.FirstOrDefault(e => e.Dishid == id);
                                if (dish == null)
                                    return false;
                                return preOrderedIds.Contains((int)id);
                            }
                            )
                            .Select(id => id!.Value)
                            .ToList();
            if (updateIds.Any())
            {
                List<Dish> dishesToUpdate = _db.Dishes
                    .Where(e => updateIds.Contains(e.Dishid))
                    .ToList();
                foreach (var item in dishesToUpdate)
                {
                    OrderedItem? updatecurrDish = model.CurrOrder.FirstOrDefault(e => e.Id == item.Dishid);
                    if (updatecurrDish == null)
                    {
                        continue;
                    }
                    Dish? dish = _db.Dishes.FirstOrDefault(e => e.Dishid == item.Dishid);
                    if (dish == null || (dish.Readyquantity + dish.Inservedquantity) > updatecurrDish.Quantity)
                    {
                        continue;
                    }
                    dish.Quantity = (short)updatecurrDish.Quantity!;
                    if (orderStatus == "Pending")
                    {
                        dish.Pendingquantity = (short)updatecurrDish.Quantity!;
                        dish.Inprogressquantity = 0;
                        dish.Readyquantity = 0;
                    }
                    else
                    {
                        dish.Pendingquantity = 0;
                        dish.Readyquantity = dish.Readyquantity;
                        dish.Inprogressquantity = (short)(dish.Quantity - (dish.Readyquantity + dish.Inservedquantity))!;
                    }
                    if (orderStatus != "Pending")
                    {
                        order.Orderstatusid = _db.Orderstatuses.FirstOrDefault(e => e.Statusname == "In Progress")!.Orderstatusid;
                    }
                    dish.Iteminstruction = updatecurrDish.Instruction;

                }
                await _db.SaveChangesAsync();
            }
            // add new items to the order--------------------------
            List<int?> newaddDishIds = currentOrderIds
                                .Where(id => id != null && !preOrderedIds.Contains((int)id))
                                .ToList();
            if (newaddDishIds.Any())
            {
                foreach (var item in newaddDishIds)
                {
                    OrderedItem? currDish = model.CurrOrder.FirstOrDefault(e => e.Id == item);
                    if (currDish == null)
                    {
                        continue;
                    }
                    Item? itemEntity = _db.Items.FirstOrDefault(e => e.Itemid == currDish.ItemId);
                    if (itemEntity == null)
                    {
                        continue;
                    }
                    Dish newdish = new Dish
                    {
                        Orderid = order.Orderid,
                        Itemid = itemEntity.Itemid,
                        Itemname = itemEntity.Itemname,
                        Price = (short)itemEntity.Rate!,
                        Quantity = (short)currDish.Quantity!,
                        Iteminstruction = currDish.Instruction,
                        Pendingquantity = (orderStatus == "Pending") ? (short)currDish.Quantity! : (short)0,
                        Inprogressquantity = (orderStatus != "Pending") ? (short)currDish.Quantity! : (short)0,
                        Readyquantity = 0,
                        Inservedquantity = 0
                    };
                    _db.Dishes.Add(newdish);

                    if (orderStatus != "Pending")
                    {
                        order.Orderstatusid = _db.Orderstatuses.FirstOrDefault(e => e.Statusname == "In Progress")!.Orderstatusid;
                    }
                    await _db.SaveChangesAsync();

                    // add modifiers to the order--------------------------------------------------
                    if (currDish.ModifierList != null && currDish.ModifierList.Any())
                    {
                        List<Dishmodifier> modifier = new List<Dishmodifier>();

                        for (int i = 0; i < currDish.ModifierList.Count; i++)
                        {
                            OrderedModifier? currModifier = currDish.ModifierList[i];
                            if (currModifier == null)
                            {
                                continue;
                            }
                            Modifier? modifierEntity = _db.Modifiers.FirstOrDefault(e => e.Modifiersid == currModifier.ModifierId);
                            if (modifierEntity == null)
                            {
                                continue;
                            }
                            Dishmodifier newModifier = new Dishmodifier
                            {
                                Dishid = newdish.Dishid,
                                Modifierid = modifierEntity.Modifiersid,
                                Modifiername = modifierEntity.Modifiersname,
                                Price = (short)modifierEntity.Modifiersrate!,
                                Quantity = 1
                            };
                            modifier.Add(newModifier);
                        }
                        if (modifier.Count > 0)
                        {
                            _db.Dishmodifiers.AddRange(modifier);
                            await _db.SaveChangesAsync();
                        }

                    }


                }
            }
            await CalculateTaxesForOrderAsync(order.Orderid);
            if (isReadyToServed(order.Orderid))
            {
                order.Orderstatusid = _db.Orderstatuses.FirstOrDefault(e => e.Statusname == "Served")!.Orderstatusid;
                order.EditDate = DateTime.Now;
                order.EditedBy = model.By;
                order.Completedtime = DateTime.Now;
                await _db.SaveChangesAsync();
            }
            await transaction.CommitAsync();
            return message;

        }

        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine("Exception" + ex);
            return ex.Message;
        }

    }

    private bool isReadyToServed(int OrderId)
    {
        return !_db.Dishes.Any(e => e.Orderid == OrderId && e.Inservedquantity != e.Quantity);
    }

    // calculate taxes ..........
    // private async Task CalculateTaxesForOrder(int orderId)
    // {
    //     Order? order = await _db.Orders.FirstOrDefaultAsync(e=>e.Orderid == orderId);
    //     if(order == null)
    //         return;

    //     List<Dish> orderDishes = await _db.Dishes
    //                             .Where(d => d.Orderid == orderId)
    //                             .ToListAsync();

    //     decimal subTotal = 0;
    //     foreach(var dish in orderDishes)
    //     {
    //         decimal modifierTotal = 0;
    //         List<Dishmodifier> modifierDishes = await _db.Dishmodifiers
    //                                             .Where(e=>e.Dishid == dish.Dishid)
    //                                             .ToListAsync();

    //         foreach(var modifier in modifierDishes)
    //         {
    //             modifierTotal += modifier.Quantity * modifier.Price;
    //         }
    //         subTotal += (dish.Price + modifierTotal) * dish.Quantity;
    //     }

    //     order.Totalamount = subTotal;
    //     await _db.SaveChangesAsync();
    // }

    public async Task<decimal> CalculateTaxesForOrderAsync(int orderId)
    {
        // using var connection = _db.Database.GetDbConnection();
        // await connection.OpenAsync();

        var sql = "SELECT calculate_order_total_menuapp(@orderId)";
        var totalAmount = await _db.Database.GetDbConnection().ExecuteScalarAsync<decimal>(sql, new { orderId });

        return totalAmount;
    }

    // public async Task<int> CreateOrder(TokenOrOrderDetails model)
    // {
    //     Order order = new Order
    //     {
    //         Customerid = (int)model.CustomerId!,
    //         Orderstatusid = _db.Orderstatuses.FirstOrDefault(e => e.Statusname == "Pending")!.Orderstatusid,
    //         Paymentid = _db.Payments.FirstOrDefault(e => e.Paymentmode == "Pending")!.Paymentid,
    //         CreatedBy = model.By!,
    //         CreatedDate = DateTime.Now
    //     };
    //     _db.Orders.Add(order);
    //     _db.SaveChanges();

    //     // create order table mapping
    //     List<int> tableIds = new List<int>();
    //     tableIds = _db.MapTableTokens
    //                     .Where(e => e.Tokenid == model.Id)
    //                     .Select(e => e.Tableid)
    //                     .ToList();

    //     await _waitingList.deleteToken((int)model.Id!);

    //     foreach (var item in tableIds)
    //     {
    //         _db.MapOrderTables.Add(new MapOrderTable
    //         {
    //             Orderid = order.Orderid,
    //             Tablesid = item
    //         });
    //         Table table = _db.Tables.FirstOrDefault(e => e.Tablesid == item)!;
    //         if (table != null)
    //         {
    //             table.Isoccupied = true;
    //             table.Isrunning = true;
    //             table.Currenttokenid = order.Orderid;
    //             _db.Tables.Update(table);
    //         }
    //     }
    //     await _db.SaveChangesAsync();

    //     return order.Orderid;

    // }
    public async Task<int> CreateOrder(TokenOrOrderDetails model)
    {
        // using var connection = _db.Database.GetDbConnection();
        // await connection.OpenAsync();

        var parameters = new DynamicParameters();
        parameters.Add("p_customer_id", model.CustomerId);
        parameters.Add("p_token_id", model.Id);
        parameters.Add("p_created_by", model.By);
        parameters.Add("p_order_id", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await _db.Database.GetDbConnection().ExecuteAsync("sp_create_order_from_token", parameters, commandType: CommandType.StoredProcedure);

        return parameters.Get<int>("p_order_id");
    }

    // public async Task<(bool, string, int)> UpdateCustomerDetails(CustomerDetailsOrderApp model)
    // {
    //     Customer? customer = await _db.Customers.FirstOrDefaultAsync(e => e.Customeremail.ToLower().Trim() == model.CustomerEmail.ToLower().Trim());
    //     if (customer == null)
    //     {
    //         return (false, "Customer not found", customer!.Customerid);
    //     }
    //     else
    //     {
    //         customer.Customername = model.CustomerName!;
    //         customer.PhoneNumber = model.MobileNo;


    //         _db.Customers.Update(customer);
    //     }
    //     await _db.SaveChangesAsync();
    //     if (model.TokenOrOrder == "token")
    //     {
    //         WaitingTable? token = await _db.WaitingTables.FirstOrDefaultAsync(e => e.Waitingid == model.Id);

    //         if (token == null)
    //             return (false, "Token not found", -1);

    //         token.Customerid = customer.Customerid;
    //         token.Totalperson = (short)model.NumberOfPersons!;
    //     }
    //     else if (model.TokenOrOrder == "order")
    //     {
    //         Order? order = await _db.Orders.FirstOrDefaultAsync(e => e.Orderid == model.Id);
    //         if (order == null)
    //             return (false, "Order not found", -1);

    //         order.Customerid = customer.Customerid;
    //         order.Personcount = (short)model.NumberOfPersons!;
    //     }
    //     else
    //     {
    //         return (false, "Invalid token or order", -1);
    //     }
    //     return (true, "Customer details updated successfully", customer.Customerid);

    // }
    public async Task<(bool, string, int)> UpdateCustomerDetails(CustomerDetailsOrderApp model)
    {
        return await ExecuteWithMultipleOutputsAsync("(CALL sp_update_customer_details)", param =>
        {
            param.Add("p_email", model.CustomerEmail);
            param.Add("p_name", model.CustomerName);
            param.Add("p_mobile", model.MobileNo);
            param.Add("p_token_or_order", model.TokenOrOrder);
            param.Add("p_id", model.Id);
            param.Add("p_person_count", model.NumberOfPersons);
            param.Add("p_success", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            param.Add("p_message", dbType: DbType.String, size: 255, direction: ParameterDirection.Output);
            param.Add("p_customer_id", dbType: DbType.Int32, direction: ParameterDirection.Output);
        },
        param => (
            param.Get<bool>("p_success"),
            param.Get<string>("p_message"),
            param.Get<int>("p_customer_id")
        ));
    }

    public bool IsOrderCompletedToServed(int orderId)
    {
        // Find the "Served" status first
        var servedStatus = _db.Orderstatuses.FirstOrDefault(status => status.Statusname == "Served");
        if (servedStatus == null)
            return false; // If "Served" status not found, return false safely

        bool orderIsServed = _db.Orders.Any(order =>
            order.Orderid == orderId && order.Orderstatusid == servedStatus.Orderstatusid);

        return orderIsServed;
    }
public async Task<bool> CompleteOrder(CompleteOrderApp model)
{
    try
    {
       
        var parameters = new DynamicParameters();
        parameters.Add("p_order_id", model.OrderId, DbType.Int32);
        parameters.Add("p_payment_method", model.PaymentMethod, DbType.String);
        parameters.Add("p_by", model.By, DbType.String);
        parameters.Add("p_food_rating", model.Food, DbType.Int32);
        parameters.Add("p_service_rating", model.Service, DbType.Int32);
        parameters.Add("p_ambience_rating", model.Ambience, DbType.Int32);
        parameters.Add("p_comments", model.Comments, DbType.String);

        // NEW: Add tax ID list as comma-separated string
        var taxIds = model.TaxList != null && model.TaxList.Any()
            ? string.Join(",", model.TaxList)
            : string.Empty;

        parameters.Add("p_tax_ids", taxIds, DbType.String);

        await _db.Database.GetDbConnection().ExecuteAsync(
            " sp_complete_order",
            parameters,
            commandType: CommandType.StoredProcedure // use Text, not StoredProcedure for CALL
        );

        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error executing stored procedure: {ex.Message}");
        return false;
    }
}


    // public async Task<bool> CompleteOrder(CompleteOrderApp model)
    // {
    //     using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
    //     try
    //     {
    //         // Check if the order is in a state that can be completed
    //         if (!IsOrderCompletedToServed(model.OrderId))
    //             return false;

    //         // Find the order
    //         var order = await _db.Orders.FirstOrDefaultAsync(o => o.Orderid == model.OrderId);
    //         if (order == null)
    //             return false;

    //         // Get the "Completed" order status
    //         var completedStatus = await _db.Orderstatuses.FirstOrDefaultAsync(s => s.Statusname == "Completed");
    //         if (completedStatus == null)
    //             return false;

    //         order.Orderstatusid = completedStatus.Orderstatusid;
    //         if (!string.IsNullOrEmpty(model.PaymentMethod))
    //         {
    //             var payment = await _db.Payments.FirstOrDefaultAsync(p => p.Paymentmode == model.PaymentMethod);
    //             if (payment != null)
    //             {
    //                 order.Paymentid = payment.Paymentid;
    //             }
    //         }
    //         order.EditedBy = model.By;
    //         order.EditDate = DateTime.Now;
    //         order.Completedtime = DateTime.Now;
    //         order.Rating = (short?)CalculateAverageRating(model.Food, model.Service, model.Ambience);
    //         order.Comments = model.Comments;


    //         _db.Orders.Update(order);

    //         // Free up the tables linked to this order
    //         await ReleaseTablesOnComplete(model.OrderId);

    //         // Remove any existing invoice and taxes
    //         await RemoveOldInvoiceIsExist(model.OrderId);

    //         // Create a new invoice
    //         var invoice = new Invoice
    //         {
    //             Orderid = model.OrderId,
    //             Paidon = DateTime.Now
    //         };
    //         _db.Invoices.Add(invoice);
    //         await _db.SaveChangesAsync();

    //         // Generate a unique invoice number
    //         var createdTime = order.CreatedDate;
    //         invoice.Invoicenumber = $"#DOM{createdTime:yyyyMMddHHmmss}{invoice.Invoiceid}";
    //         _db.Invoices.Update(invoice);

    //         // Add taxes if any
    //         if (model.TaxList != null && model.TaxList.Any())
    //         {
    //             foreach (var taxId in model.TaxList)
    //             {
    //                 var tax = await _db.Taxes.FirstOrDefaultAsync(t => t.Taxesid == taxId);
    //                 if (tax == null)
    //                     continue;

    //                 var invoiceTax = new Invoicetax
    //                 {
    //                     Invoiceid = invoice.Invoiceid,
    //                     Taxid = tax.Taxesid,
    //                     Taxname = tax.Taxname,
    //                     Taxvalue = (decimal)tax.Taxvalue!,
    //                     Taxvaluetype = tax.Taxtype!
    //                 };
    //                 _db.Invoicetaxes.Add(invoiceTax);
    //             }
    //         }

    //         await _db.SaveChangesAsync();

    //         // Calculate and update final taxes
    //         await CalculateTaxesForCompleteOrder(order.Orderid);

    //         await transaction.CommitAsync();
    //         return true;
    //     }
    //     catch (Exception ex)
    //     {
    //         await transaction.RollbackAsync();
    //         Console.WriteLine($"Error completing order: {ex.Message}");
    //         return false;
    //     }
    // }
    private int? CalculateAverageRating(int food, int service, int ambience)
    {
        var scores = new[] { food, service, ambience }.Where(x => x > 0);
        return scores.Any() ? (int?)Math.Round(scores.Average(), MidpointRounding.AwayFromZero) : null;
    }

    private async Task ReleaseTablesOnComplete(int orderId)
    {
        await _db.Database.GetDbConnection().ExecuteAsync("CALL sp_release_tables_on_complete(@p_order_id)", 
            new { p_order_id = orderId });
        // var tableIds = await _db.MapOrderTables
        //                         .Where(map => map.Orderid == orderId)
        //                         .Select(map => map.Tablesid)
        //                         .ToListAsync();

        // foreach (var id in tableIds)
        // {
        //     var table = await _db.Tables.FirstOrDefaultAsync(t => t.Tablesid == id);
        //     if (table != null)
        //     {
        //         table.Isoccupied = false;
        //         table.Isrunning = null;
        //         table.Currenttokenid = null; 
        //         _db.Tables.Update(table);
        //     }
        // }
        // await _db.SaveChangesAsync();
    }


    private async Task RemoveOldInvoiceIsExist(int orderId)
    {
        // var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.Orderid == orderId);
        // if (invoice != null)
        // {
        //     var invoiceTaxes = await _db.Invoicetaxes.Where(it => it.Invoiceid == invoice.Invoiceid).ToListAsync();
        //     _db.Invoicetaxes.RemoveRange(invoiceTaxes);
        //     _db.Invoices.Remove(invoice);
        //     await _db.SaveChangesAsync();
        // }
        await _db.Database.GetDbConnection().ExecuteAsync("CALL sp_remove_old_invoice_if_exists(@p_order_id)",
            new { p_order_id = orderId });
    }

    // calculate taxes for complete order ..........
    // private async Task CalculateTaxesForCompleteOrder(int orderId)
    // {
    //     // calcualte subtotal .......
    //     Order? order = await _db.Orders.FirstOrDefaultAsync(e => e.Orderid == orderId);
    //     if (order == null)
    //         return;

    //     List<Dish> orderDishes = await _db.Dishes
    //                             .Where(d => d.Orderid == orderId)
    //                             .ToListAsync();

    //     decimal subTotal = 0;
    //     foreach (var dish in orderDishes)
    //     {
    //         decimal modifierTotal = 0;
    //         List<Dishmodifier> modifierDishes = await _db.Dishmodifiers
    //                                             .Where(e => e.Dishid == dish.Dishid)
    //                                             .ToListAsync();

    //         foreach (var modifier in modifierDishes)
    //         {
    //             modifierTotal += modifier.Quantity * modifier.Price;
    //         }
    //         subTotal += (dish.Price + modifierTotal) * dish.Quantity;
    //     }

    //     // calaculate total taxes ..........
    //     Invoice? invoice = _db.Invoices.FirstOrDefault(e => e.Orderid == orderId);
    //     if (invoice != null)
    //     {
    //         List<Invoicetax> taxMaps = await _db.Invoicetaxes
    //                                     .Where(e => e.Invoiceid == invoice.Invoiceid)
    //                                     .ToListAsync();

    //         decimal taxSum = 0;

    //         foreach (var taxMap in taxMaps)
    //         {
    //             if (taxMap.Taxvaluetype == "Percentage")
    //             {
    //                 taxSum += Math.Round(subTotal * taxMap.Taxvalue / 100, 2);
    //             }
    //             else
    //             {
    //                 taxSum += Convert.ToDecimal(taxMap.Taxvalue);
    //             }
    //         }

    //         subTotal += taxSum;
    //     }



    //     order.Totalamount = subTotal;
    //     await _db.SaveChangesAsync();
    // }
    private async Task CalculateTaxesForCompleteOrder(int orderId)
    {
        using var connection = _db.Database.GetDbConnection();
        await connection.OpenAsync();

        var parameters = new DynamicParameters();
        parameters.Add("p_order_id", orderId);

        await connection.ExecuteAsync("CALL sp_calculate_order_total_with_tax(@p_order_id)", parameters);
    }
    public async Task<bool> CancelOrderApp(int orderId, string cancelledBy)
    {
        try
        {
            await _db.Database.GetDbConnection().ExecuteAsync(
                "CALL sp_cancel_order_app(@p_order_id, @p_cancelled_by)",
                new { p_order_id = orderId, p_cancelled_by = cancelledBy }
            );
            
            // var order = await _db.Orders.FirstOrDefaultAsync(o => o.Orderid == orderId);
            // if (order == null)
            //     return false;

            // bool canCancel = !_db.Dishes.Any(d =>
            //                     d.Orderid == orderId &&
            //                     (
            //                         (d.Readyquantity + d.Inservedquantity) > 0 ||
            //                         (d.Pendingquantity + d.Inprogressquantity) < d.Quantity
            //                     )
            //                 );

            // if (!canCancel)
            //     return false;

            // var cancelledStatus = _db.Orderstatuses.FirstOrDefault(s => s.Statusname == "Cancelled");
            // if (cancelledStatus == null)
            //     return false;

            // order.Orderstatusid = cancelledStatus.Orderstatusid;
            // order.EditedBy = cancelledBy;
            // order.EditDate = DateTime.Now;
            // order.Completedtime = DateTime.Now;

            // await ReleaseTablesOnComplete(orderId);

            return true;
        }
        catch
        {
            return false;
        }
    }
    

    public async Task<TOut> ExecuteWithMultipleOutputsAsync<TOut>(string procName, Action<DynamicParameters> buildParams, Func<DynamicParameters, TOut> mapOutputs)
    {
        using var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
 
        var parameters = new DynamicParameters();
        buildParams(parameters);
 
        await conn.ExecuteAsync(procName, parameters, commandType: CommandType.StoredProcedure);
 
        return mapOutputs(parameters);
    }
 

}
