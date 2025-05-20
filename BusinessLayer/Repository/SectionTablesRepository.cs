using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BusinessLayer.Interface;
using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Repository;

public class SectionTablesRepository : ISectionTables
{
    private readonly PizzaShopContext _db;

    public SectionTablesRepository(PizzaShopContext db)
    {
        _db = db;
    }

    public string? GetEmailFromToken(HttpRequest request)
    {
        var token = request.Cookies["JWTLogin"];
        if (string.IsNullOrEmpty(token))
            return null;

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var email = jwtToken.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Name)?.Value;
        return email;
    }
    public List<SectionViewModal> GetSections()
    {
        return _db.Sections
            .Where(c => c.Isdeleted != true)
            .OrderBy(c => c.Sectionid)
            .Select(c => new SectionViewModal
            {
                SectionId = c.Sectionid,
                Sectionname = c.Sectionname,
                Sectiondescription = c.Sectiondescription
            }).ToList();
    }

    public void AddSections(Section sections, string email)
    {
        var existingSection = _db.Sections.FirstOrDefault(c => c.Sectionname.ToLower().Trim() == sections.Sectionname.ToLower().Trim() );
        if (existingSection != null)
        {
            if ((bool)existingSection.Isdeleted!)
            {
                existingSection.Isdeleted = false;
                existingSection.Sectiondescription = sections.Sectiondescription;
                _db.SaveChanges();
            }
            else
            {
                throw new Exception("Section name already exists!");
            }
        }
        else
        {
            var newSection = new Section
            {
                Sectionname = sections.Sectionname,
                Sectiondescription = sections.Sectiondescription,
                CreatedBy = email
            };
            _db.Sections.Add(newSection);
            _db.SaveChanges();
        }
    }

    // Retrieves a specific section by its ID
    public AddEditSectionViewModal GetSectionById(int id)
    {
        var sections = _db.Sections.FirstOrDefault(c => c.Sectionid == id);
        if (sections == null) throw new KeyNotFoundException($"Section with ID {id} not found.");

        return new AddEditSectionViewModal
        {
            SectionId = sections.Sectionid,
            Sectionname = sections.Sectionname,
            Sectiondescription = sections.Sectiondescription
        };
    }

    public void UpdateSection(Section sections, string email)
    {
        var existingSection = _db.Sections.FirstOrDefault(s => s.Sectionid == sections.Sectionid);
        if (existingSection != null)
        {
            existingSection.Sectionname = sections.Sectionname;
            existingSection.Sectiondescription = sections.Sectiondescription;
            existingSection.EditDate = DateTime.Now;
            existingSection.EditedBy = email;
            _db.SaveChanges();
        }
    }

    // Performs a soft delete on a section 
    public bool SoftDeleteSection(int sectionId)
    {
        var sections = _db.Sections.FirstOrDefault(s => s.Sectionid == sectionId);
        bool tables = _db.Tables.Any(t => t.Sectionid == sectionId && t.Isoccupied == true);
        if (sections != null && !tables )
        {
            sections.Isdeleted = true;
            _db.SaveChanges();
            return true;
        }
        return false;
    }

    public Pagination<TablesView> GetTableBySection(int sectionId, int PageNumber = 1, int PageSize = 5, string SearchKey = "")
    {
        Pagination<TablesView> newmodel = new Pagination<TablesView>();
        var query = _db.Tables
             .Where(i => i.Sectionid == sectionId && i.Isdeleted != true && i.Tablename.ToLower().Contains(SearchKey.ToLower()))
             .Select(i => new TablesView
             {
                 TablesId = i.Tablesid,
                 Tablename = i.Tablename,
                 Tablecapacity = i.Tablecapacity,
                 Isoccupied = i.Isoccupied,
                 SectionId = i.Sectionid

             });
        if (PageNumber < 1)
        {
            PageNumber = 1;
        }
        newmodel.Count = query.Count();
        newmodel.MaxPage = (int)Math.Ceiling(newmodel.Count / (double)PageSize);

        newmodel.PageSize = PageSize;

        if (PageNumber > newmodel.MaxPage)
        {
            PageNumber = newmodel.MaxPage;
        }
        if (PageNumber < 1)
        {
            PageNumber = 1;
        }
        newmodel.PageNumber = PageNumber;
        newmodel.ParentId = sectionId;
        newmodel.SearchKey = SearchKey;
        newmodel.List = query.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToList();
        return newmodel;
    }

    public string AddTable(AddEditTablesView model, string email)
    {
        try
        {
            var existingTable = _db.Tables.FirstOrDefault(t => t.Tablename.ToLower().Trim() == model.Tablename.ToLower().Trim() && t.Sectionid == model.SectionId);
            if (existingTable != null)
            {
                if ((bool)existingTable.Isdeleted!)
                {
                    existingTable.Tablename = model.Tablename;
                    existingTable.Tablecapacity = model.Tablecapacity;
                    existingTable.Isoccupied = false;
                    existingTable.Sectionid = model.SectionId;
                    existingTable.CreatedBy = email;
                    existingTable.Isdeleted= false;
                    _db.Tables.Update(existingTable); 
                    _db.SaveChanges();
                    return "Table restored successfully!";
                }
                else
                {
                    return "Table name already exists!";
                }
            }
            else
            {
                var newTable = new Table
                {
                    Tablename = model.Tablename,
                    Tablecapacity = model.Tablecapacity,
                    Isoccupied = model.Isoccupied,
                    Sectionid = model.SectionId,
                    CreatedBy = email
                };
                _db.Tables.Add(newTable);
                _db.SaveChanges();
                return "Table added successfully!";
            }
        }
        catch (Exception)
        {
            return "An error occurred while adding the table.";
        }
    }

    public async Task<AddEditTablesView> GetTableByIdAsync(int tableId)
    {
        var table = await _db.Tables
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Tablesid == tableId);

        if (table == null)
        {
            return null;
        }

        return new AddEditTablesView
        {
            TablesId = table.Tablesid,
            Tablename = table.Tablename,
            Tablecapacity = table.Tablecapacity,
            Isoccupied = table.Isoccupied, // Ensure this field has a value
            SectionId = table.Sectionid
        };
    }

    public bool UpdateTable(AddEditTablesView table)
    {
        var existingTable = _db.Tables.Find(table.TablesId);
        if (existingTable != null)
        {
            existingTable.Tablename = table.Tablename;
            existingTable.Tablecapacity = table.Tablecapacity;
            existingTable.Isoccupied = table.Isoccupied;
            existingTable.Sectionid = table.SectionId;

            _db.SaveChanges();
            return true;
        }
        return false;
    }
    public bool SoftDeleteTable(List<int> tableIds)
    {
        var items = _db.Tables.Where(i => tableIds.Contains(i.Tablesid) && i.Isoccupied==false).ToList();
        if (items.Any())
        {
            foreach (var item in items)
            {
                item.Isdeleted = true;
            }
            _db.SaveChanges();
            return true;
        }
        return true;
    }

    public (string,bool) GetMessage (List<int> tableIds)
    {
        var items = _db.Tables.Where(i => tableIds.Contains(i.Tablesid) && i.Isoccupied==true).ToList();
        List<string> message = new List<string>(); 

        if(items.Any())
        {
            foreach (var item in items)
            {
                message.Add(item.Tablename);
            }
            message.Add("are occupied");
            message.Add("and cannot be deleted");
            return (string.Join(", ", message), false);
        }
        else
        {
            return ("All items deleted successfully", true);
        }

    }

    public (string, bool) GetMessageSection(int sectionId)
    {
        var items = _db.Tables.Where(i => i.Sectionid == sectionId && i.Isoccupied == true).ToList();
        List<string> message = new List<string>();

        if (items.Any())
        {
            foreach (var item in items)
            {
                message.Add(item.Tablename);
            }
            message.Add("are occupied");
            message.Add("and cannot be deleted");
            return (string.Join(", ", message), false);
        }
        else
        {
            return ("All items deleted successfully", true);
        }

    }

}
