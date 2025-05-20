using DataAccessLayer.ViewModels;
using Microsoft.AspNetCore.Http;

namespace BusinessLayer.Interface;

public interface ITaxesFees
{
    string? GetEmailFromToken(HttpRequest request);

    public List<TaxesView> GetAllTaxesFees(string SearchKey="");

    bool AddTax(AddEditTaxes model, string email);

    TaxesFeesViewModel GetTaxById(int id); 
    bool UpdateTax(TaxesFeesViewModel tax); // Update tax details

    Task DeleteTax(int taxId);

}
