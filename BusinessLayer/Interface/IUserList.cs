using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;
using Microsoft.AspNetCore.Http;

namespace BusinessLayer.Interface;

public interface IUserList
{
    Task<PaginationParams>GetUsers(PaginationParams model, string email);
    Task<string> AddUser(AddUserViewModel model, string email); // Method to add a user

    Task<bool> AddUserEmail(string newEmail,string newPassword, string callbackUrl);

    List<Role> GetRoles();
    List<Country> GetCountries();
    List<State> GetStatesByCountry(int countryId);
    List<City> GetCitiesByState(int stateId);
    Task<EditUserViewModel> GetUserProfileDetailsAsync(int userId);
    Task<bool> EditUserProfileDetailsAsync(EditUserViewModel model);
    Task DeleteUser(int userId);
    public Task<string?> UploadPhotoAsync(IFormFile photo);

}
