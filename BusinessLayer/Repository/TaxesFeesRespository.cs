using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BusinessLayer.Interface;
using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Repository;

public class TaxesFeesRespository : ITaxesFees
{
    private readonly PizzaShopContext _db;
    public TaxesFeesRespository(PizzaShopContext db)
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
    public List<TaxesView> GetAllTaxesFees(string SearchKey="")
    {
        return _db.Taxes
            .Where(t => !(t.Isdeleted ?? false) && (t.Taxname.ToLower().Contains(SearchKey.ToLower()) || t.Taxtype.ToLower().Contains(SearchKey.ToLower())))
            .Select(t => new TaxesView
            {
                TaxesId = t.Taxesid,
                Taxname = t.Taxname,
                Taxtype = t.Taxtype,
                Isenabled = t.Isenabled,
                Isdefault = t.Isdefault,
                Taxvalue = t.Taxvalue
            }).ToList();
    }

    public bool AddTax(AddEditTaxes model, string email)
    {
        var existingTax = _db.Taxes
            .FirstOrDefault(t => t.Taxname.ToLower().Trim() == model.Taxname.ToLower().Trim());
        if (existingTax != null)
        {
            if (existingTax.Isdeleted == true)
            {
                existingTax.Isdeleted = false;
                existingTax.Taxname = model.Taxname;
                existingTax.Taxtype = model.Taxtype;
                existingTax.Isenabled = model.Isenabled;
                existingTax.Isdefault = model.Isdefault;
                existingTax.Taxvalue = model.Taxvalue;
                existingTax.EditedBy = email;
                return _db.SaveChanges() > 0;
            }
            else
            {
                return false; // Tax already exists and is not deleted
            }
        }
        else
        {

            var newTax = new Taxis
            {
                Taxname = model.Taxname,
                Taxtype = model.Taxtype,
                Isenabled = model.Isenabled ,
                Isdefault = model.Isdefault ,
                Taxvalue = model.Taxvalue,
                CreatedBy = email,
            
            };

            _db.Taxes.Add(newTax);

            return _db.SaveChanges() > 0;
        }
    }

    public TaxesFeesViewModel GetTaxById(int id)
    {
        var tax = _db.Taxes.FirstOrDefault(t => t.Taxesid == id);
        if (tax == null) return null;

        return new TaxesFeesViewModel
        {
            AddEditTaxe = new AddEditTaxes
            {
                TaxesId = tax.Taxesid,
                Taxname = tax.Taxname,
                Taxtype = tax.Taxtype,
                Taxvalue = tax.Taxvalue,
                Isenabled = (bool)tax.Isenabled,
                Isdefault = (bool)tax.Isdefault
            }
        };
    }
    public bool UpdateTax(TaxesFeesViewModel taxViewModel)
    {
        var tax = _db.Taxes.FirstOrDefault(t => t.Taxesid == taxViewModel.AddEditTaxe.TaxesId);
        if (tax != null)
        {
            tax.Taxname = taxViewModel.AddEditTaxe.Taxname;
            tax.Taxtype = taxViewModel.AddEditTaxe.Taxtype;
            tax.Taxvalue = taxViewModel.AddEditTaxe.Taxvalue;
            tax.Isenabled = taxViewModel.AddEditTaxe.Isenabled;
            tax.Isdefault = taxViewModel.AddEditTaxe.Isdefault;

            _db.SaveChanges();
            return true;
        }
        else
        {
            return false;
        }
    }

    // Soft deletes a tax
    public async Task DeleteTax(int taxId)
    {
        var tax = await _db.Taxes.FirstOrDefaultAsync(u => u.Taxesid == taxId);
        if (tax != null)
        {
            tax.Isdeleted = true;
            _db.Taxes.Update(tax);
            await _db.SaveChangesAsync();
        }
    }


}
