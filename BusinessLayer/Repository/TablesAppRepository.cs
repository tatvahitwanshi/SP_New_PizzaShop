using System.Transactions;
using BusinessLayer.Interface;
using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;
using Microsoft.EntityFrameworkCore;
namespace BusinessLayer.Repository;

public class TablesAppRepository : ITablesApp
{
    private readonly PizzaShopContext _db;

    public TablesAppRepository(PizzaShopContext db)
    {
        _db = db;
    }

    public  async Task<List<SectionOrderView>> getTableOrders()
    {
        List<SectionOrderView> sectionOrders = new List<SectionOrderView>();

        sectionOrders = await (from section in _db.Sections
                        where section.Isdeleted == false
                        orderby section.Sectionid
                        select new SectionOrderView
                        {
                            SectionId = section.Sectionid,
                            SectionName = section.Sectionname,
                            TableList = (from table in _db.Tables
                                         where table.Sectionid == section.Sectionid && table.Isdeleted == false
                                         orderby table.Tablesid
                                         select new TableOrderView
                                         {
                                             TableId = table.Tablesid,
                                             TableName = table.Tablename,
                                             Capacity = Convert.ToInt32(table.Tablecapacity),
                                             isOccupied = table.Isoccupied,
                                             isRunning = (bool)table.Isoccupied ? table.Isrunning : null,
                                             OrderTokenId = (bool)table.Isoccupied ? table.Currenttokenid : null                                             
                                         }).ToList()

                        }).ToListAsync();

        foreach (var section in sectionOrders)
        {
            section.AvailableCount = 0;
            section.AssignedCount = 0;
            section.RunningCount = 0;

            if (section.TableList != null)
            {
                foreach(var table in section.TableList)
                {
                    if(table.isOccupied != null && table.isRunning != null && table.OrderTokenId != null)
                    {
                        table.OrderAmount = await _db.Orders
                                                    .Where(x => x.Orderid == table.OrderTokenId)
                                                    .Select(x => x.Totalamount)
                                                    .FirstOrDefaultAsync();
                    }

                    if(table.isOccupied != null && table.isOccupied == true)
                    {
                        if(table.isRunning != null && table.isRunning == true)
                        {
                            table.Time = await _db.Orders
                                                .Where(x => x.Orderid == table.OrderTokenId)
                                                .Select(x => x.CreatedDate)
                                                .FirstOrDefaultAsync();
                            table.Status = "running";
                            section.RunningCount += 1;
                        }
                        else if(table.isRunning != null && table.isRunning == false)
                        {
                            table.Time = await _db.WaitingTables
                                                .Where(x => x.Waitingid == table.OrderTokenId)
                                                .Select(x => x.Assigntime)
                                                .FirstOrDefaultAsync();
                            table.Status = "assigned";
                            section.AssignedCount += 1;
                        }
                    }else
                    {
                        Console.WriteLine(table.Time);
                        table.Status = "available";
                        section.AvailableCount += 1;
                    }
                }
            }
        }
        return sectionOrders;
    }
    private static IEnumerable<List<T>> GetCombinations<T>(List<T> list, int length)
    {
        if (length == 1)
            return list.Select(t => new List<T> { t });

        return list.SelectMany((item, index) =>
            GetCombinations(list.Skip(index + 1).ToList(), length - 1)
                .Select(sub => new List<T> { item }.Concat(sub).ToList())
        );
    }


    public async Task<(bool, string)> assignToken(WaitingToken model, List<int>? tableIdsForAssign)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            bool status = true;
            string message= "Tables Assigned Successfully";
            // Check if customer already has an assigned table
            var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Customeremail.ToLower().Trim() == model.CustomerEmail.ToLower().Trim());
            if (customer != null)
            {
                bool alreadyAssigned = await _db.WaitingTables
                    .AnyAsync(wt => wt.Customerid == customer.Customerid && wt.Isassigned == true);

                if (alreadyAssigned)
                {
                    return (false, "Customer already has an assigned table.");
                }
            }
            
            
            if (tableIdsForAssign != null && tableIdsForAssign.Count > 0)
            {
                var availableTables = _db.Tables
                    .Where(t => tableIdsForAssign.Contains(t.Tablesid) && !(t.Isoccupied ?? false))
                    .ToList() // Fetch data into memory
                    .Where(t => int.TryParse(t.Tablecapacity, out _)) 
                    .Select(t => new
                    {
                        TableId = t.Tablesid,
                        Capacity = int.Parse(t.Tablecapacity!),
                        Table = t
                    })
                    .ToList();

                int requiredCapacity = (int)model.NumberOfPersons!;

                // Get all combinations
                var bestCombination = availableTables
                    .SelectMany((_, i) => GetCombinations(availableTables, i + 1))
                    .Where(combo => combo.Sum(t => t.Capacity) >= requiredCapacity)
                    .OrderBy(combo => combo.Count())                
                    .ThenBy(combo => combo.Sum(t => t.Capacity))  
                    .FirstOrDefault();

                if (bestCombination == null)
                    return (false, "No combination of selected tables meets the required capacity.");

                // Replace tableIdsForAssign with best matched
                tableIdsForAssign = bestCombination.Select(t => t.TableId).ToList();
            }
            else
            {
                return (false, "Select Tables to Assign");
            }


            WaitingTable? token;
            if(model.TokenId != null) token= await _db.WaitingTables.FirstOrDefaultAsync(x => x.Waitingid == model.TokenId);
            else token = await generateToken(model);

            if(token != null)
            {
                token.Isassigned= true;
                token.Assigntime= DateTime.Now;
                token.EditDate= DateTime.Now;
                token.EditedBy= model.By;

                foreach(var tableId in tableIdsForAssign)
                {
                    _db.MapTableTokens.Add(new MapTableToken
                    {
                        Tableid = tableId,
                        Tokenid = token.Waitingid,
                    
                    });
                    Table? singleTable = _db.Tables.FirstOrDefault(x => x.Tablesid == tableId);
                    if(singleTable != null)
                    {
                        singleTable.Isoccupied = true;
                        singleTable.Isrunning = false;
                        singleTable.Currenttokenid = token.Waitingid;
                        singleTable.EditDate= DateTime.Now;
                        singleTable.EditedBy= model.By;
                        _db.Update(singleTable);
                    }
                }
            }
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return (status, message);
        }
        catch(Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, ex.Message);
        }
    }

    public async Task<WaitingTable?> generateToken(WaitingToken token)
    {
        //pwd
        Customer? customer = _db.Customers.FirstOrDefault(x => x.Customeremail == token.CustomerEmail);
    

        if(customer == null)
        {
            customer = new Customer
            {
                Customername = token.CustomerName!,
                PhoneNumber = token.MobileNo,
                Customeremail = token.CustomerEmail,
                CreatedDate = DateTime.Now
            };
            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();
        }else
        {
            customer.Customername = token.CustomerName!;
            customer.PhoneNumber = token.MobileNo;

            _db.Customers.Update(customer);
            await _db.SaveChangesAsync();
        }
        customer = await _db.Customers.FirstOrDefaultAsync(x => x.Customeremail == token.CustomerEmail);

        var existingWaiting = await _db.WaitingTables.AnyAsync(x => x.Customerid == customer!.Customerid && (token.TokenId == null || token.TokenId == 0 || x.Waitingid != token.TokenId));

            if (existingWaiting)
                throw new InvalidOperationException("Customer is already in the waiting list.");

        // Generate a new token
        WaitingTable? newToken = new WaitingTable
        {
            Tokennumber = _db.WaitingTables.Any()? _db.WaitingTables.Max(e => e.Tokennumber) + 1 : 1,
            Totalperson = (short)token.NumberOfPersons!,
            Tokendate = DateTime.Now,
            Sectionid = (int)token.SectionId!,
            Customerid = customer.Customerid,
            CreatedBy = token.By,
        };
        _db.WaitingTables.Add(newToken);
        await _db.SaveChangesAsync();


        return newToken;
    }

    public int getTokenId(int tableId)
    {
        Table? table = _db.Tables.FirstOrDefault(e => e.Tablesid == tableId && e.Isoccupied == true && e.Isrunning == false);
        return (int)(table != null ? table.Currenttokenid! : 0);
    }
}
