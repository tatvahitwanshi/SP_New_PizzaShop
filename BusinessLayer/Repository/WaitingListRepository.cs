using BusinessLayer.Interface;
using Dapper;
using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PizzashopRMS.Helpers;

namespace BusinessLayer.Repository;

public class WaitingListRepository : IWaitingList
{
    private readonly PizzaShopContext _db;

    public WaitingListRepository(PizzaShopContext db)
    {
        _db = db;
    }

    public async Task<List<SectionDetails>> getSectionList()
    {
        List<SectionDetails> list = await (from section in _db.Sections
                                           where section.Isdeleted != true
                                           orderby section.Sectionid
                                           select new SectionDetails
                                           {
                                               SectionId = section.Sectionid,
                                               SectionName = section.Sectionname,
                                               TotalToken = (from token in _db.WaitingTables
                                                             where token.Sectionid == section.Sectionid && token.Isassigned == false
                                                             select token).Count()
                                           }).ToListAsync();

        int totalTokensAll = await _db.WaitingTables.Where(e => e.Isassigned == false).CountAsync();
        list.Insert(0, new SectionDetails
        {
            SectionId = 0,
            SectionName = "All",
            TotalToken = totalTokensAll
        });

        return list;
    }

    public async Task<List<TokenDetail>> getTokenList(int sectionId)
    {
        List<TokenDetail> list = await (from token in _db.WaitingTables
                                        join customer in _db.Customers on token.Customerid equals customer.Customerid
                                        where (sectionId == 0 || token.Sectionid == sectionId) && token.Isassigned == false
                                        orderby token.Tokennumber
                                        select new TokenDetail
                                        {
                                            TokenId = token.Waitingid,
                                            TokenNo = token.Tokennumber.ToString(),
                                            CreatedDate = token.Tokendate,
                                            NoOfPerson = token.Totalperson,
                                            CustomerName = customer.Customername,
                                            PhoneNo = customer.PhoneNumber,
                                            Email = customer.Customeremail

                                        }).ToListAsync();

        return list;
    }
    public async Task<List<TableSingle>> getTableList(int sectionId, int capacity)
    {
        List<TableSingle> tableList = await (from table in _db.Tables
                                             where table.Sectionid == sectionId
                                                 && table.Isdeleted == false
                                                 && table.Isoccupied == false
                                                 && Convert.ToInt32(table.Tablecapacity) >= capacity
                                             orderby table.Tablename
                                             select new TableSingle
                                             {
                                                 TableId = table.Tablesid,
                                                 TableName = table.Tablename
                                             }).ToListAsync();

        return tableList;
    }

    public async Task<WaitingToken> getToken(int id)
    {
        WaitingToken token = new WaitingToken();
        token.SectionList = await (from section in _db.Sections
                                   where section.Isdeleted == false
                                   orderby section.Sectionid
                                   select new SectionDetails
                                   {
                                       SectionId = section.Sectionid,
                                       SectionName = section.Sectionname
                                   }).ToListAsync();

        if (id != 0)
        {
            WaitingTable? tokenDetails = await _db.WaitingTables.FirstOrDefaultAsync(x => x.Waitingid == id);
            if (tokenDetails != null)
            {
                token.TokenId = tokenDetails.Waitingid;
                token.NumberOfPersons = tokenDetails.Totalperson;
                token.SectionId = tokenDetails.Sectionid;

                Customer? customer = await _db.Customers.FirstOrDefaultAsync(x => x.Customerid == tokenDetails.Customerid);
                if (customer != null)
                {
                    token.CustomerEmail = customer.Customeremail;
                    token.CustomerName = customer.Customername;
                    token.MobileNo = customer.PhoneNumber;
                }
            }
        }

        return token;
    }

    // public async Task generateToken(WaitingToken token)
    // {
    //     var email = token.CustomerEmail.ToLower().Trim();

    //     // Start a transaction
    //     using var transaction = await _db.Database.BeginTransactionAsync();
    //     try
    //     {
    //         // Step 1: Check if customer exists
    //         var customer = await _db.Customers.FirstOrDefaultAsync(x => x.Customeremail.ToLower().Trim() == email);
    //         if (customer == null)
    //         {
    //             customer = new Customer
    //             {
    //                 Customername = token.CustomerName,
    //                 PhoneNumber = token.MobileNo,
    //                 Customeremail = token.CustomerEmail,
    //                 CreatedDate = DateTime.Now
    //             };
    //             _db.Customers.Add(customer);
    //             await _db.SaveChangesAsync();
    //         }
    //         else
    //         {
    //             customer.Customername = token.CustomerName;
    //             customer.PhoneNumber = token.MobileNo;
    //             _db.Customers.Update(customer);
    //             await _db.SaveChangesAsync();
    //         }

    //         // Refresh customer
    //         customer = await _db.Customers.FirstOrDefaultAsync(x => x.Customeremail.ToLower().Trim() == email);

    //         var existingWaiting = await _db.WaitingTables.AnyAsync(x => x.Customerid == customer!.Customerid && (token.TokenId == null || token.TokenId == 0 || x.Waitingid != token.TokenId));

    //         if (existingWaiting)
    //             throw new InvalidOperationException("Customer is already in the waiting list.");

    //         // Step 3: Proceed with token generation
    //         if (token.TokenId == null || token.TokenId == 0)
    //         {

    //             int nextTokenNumber = (_db.WaitingTables.Any()) ? _db.WaitingTables.Max(e => e.Tokennumber) + 1 : 1;

    //             WaitingTable newToken = new WaitingTable
    //             {
    //                 Tokennumber = nextTokenNumber,
    //                 Totalperson = (short)token.NumberOfPersons,
    //                 Sectionid = (int)(token.SectionId ?? 0),
    //                 Tokendate = DateTime.Now,
    //                 Customerid = customer.Customerid,
    //                 CreatedBy = token.By
    //             };
    //             _db.WaitingTables.Add(newToken);
    //         }
    //         else
    //         {

    //             WaitingTable? existingToken = await _db.WaitingTables.FirstOrDefaultAsync(x => x.Waitingid == token.TokenId);
    //             if (existingToken != null)
    //             {
    //                 existingToken.Totalperson = (short)token.NumberOfPersons!;
    //                 existingToken.Sectionid = (int)token.SectionId!;
    //                 existingToken.Customerid = customer.Customerid;
    //                 existingToken.EditedBy = (string)token.By!;
    //                 existingToken.EditDate = DateTime.Now;

    //                 _db.WaitingTables.Update(existingToken);
    //             }
    //         }

    //         await _db.SaveChangesAsync();
    //         await transaction.CommitAsync(); 

    //     }
    //     catch
    //     {
    //         await transaction.RollbackAsync();
    //         throw;
    //     }
    // }
    public async Task generateToken(WaitingToken token)
    {
        var email = token.CustomerEmail!.ToLower().Trim();

        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_customer_name", token.CustomerName);
            parameters.Add("p_mobile_no", token.MobileNo);
            parameters.Add("p_customer_email", token.CustomerEmail);
            parameters.Add("p_number_of_persons", token.NumberOfPersons);
            parameters.Add("p_section_id", token.SectionId ?? 0);
            parameters.Add("p_by", token.By);
            parameters.Add("p_token_id", token.TokenId ?? 0);

            await _db.Database.GetDbConnection().ExecuteAsync("CALL sp_generate_token(@p_customer_name, @p_mobile_no, @p_customer_email, @p_number_of_persons, @p_section_id, @p_by, @p_token_id)", parameters);
        }
        catch (PostgresException ex)
        {
            if (ex.SqlState == "P0001") // Matches custom ERRCODE in SP
            {
                throw new InvalidOperationException(ex.Message);
            }

            throw;
        }
    }



    public async Task assignToken(AssignToken assignToken)
    {
        WaitingTable? token = await _db.WaitingTables.FirstOrDefaultAsync(x => x.Waitingid == assignToken.TokenId);
        if (token != null)
        {
            token.Sectionid = (int)assignToken.SectionId!;
            token.Isassigned = true;
            token.Assigntime = DateTime.Now;

            _db.MapTableTokens.Add(new MapTableToken
            {
                Tokenid = token.Waitingid,
                Tableid = (int)assignToken.TableId
            });

            if (assignToken.By != null)
            {
                token.EditedBy = assignToken.By;
                token.EditDate = DateTime.Now;
            }

            Table? table = await _db.Tables.FirstOrDefaultAsync(x => x.Tablesid == assignToken.TableId);
            if (table != null)
            {
                table.Isoccupied = true;
                table.Isrunning = false;
                table.Currenttokenid = token.Waitingid;

                if (assignToken.By != null)
                {
                    table.EditedBy = assignToken.By;
                    table.EditDate = DateTime.Now;
                }
                _db.Tables.Update(table);
            }

            _db.WaitingTables.Update(token);
            await _db.SaveChangesAsync();
        }
    }

    public async Task deleteToken(int id)
    {
        WaitingTable? token = await _db.WaitingTables.FirstOrDefaultAsync(x => x.Waitingid == id);
        if (token != null)
        {
            List<int> map = await _db.MapTableTokens.Where(e => e.Tokenid == id).Select(m => m.Tableid).ToListAsync();

            if (map?.Count > 0)
            {
                List<Table>? list = _db.Tables.Where(e => map.Contains(e.Tablesid)).ToList();
                foreach (var table in list)
                {
                    table.Isoccupied = false;
                    table.Isrunning = null;
                    table.Currenttokenid = null;
                }

                _db.Tables.UpdateRange(list);
            }


            IQueryable<MapTableToken> removeMap = _db.MapTableTokens.Where(e => e.Tokenid == id);
            _db.MapTableTokens.RemoveRange(removeMap);
            _db.WaitingTables.Remove(token);
            await _db.SaveChangesAsync();
        }
        return;
    }

    public async Task<Customer?> getCustomerFromEmail(string email)
    {
        return await _db.Customers.FirstOrDefaultAsync(x => x.Customeremail.ToLower().Trim() == email.ToLower().Trim());

    }

}
