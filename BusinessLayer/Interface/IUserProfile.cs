using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;

namespace BusinessLayer.Interface
{
    public interface IUserProfile
    {
        Task<UserProfileViewModel> GetUserProfileAsync(string email);
        Task<bool> UpdateUserProfileAsync(UserProfileViewModel model);
        Task<List<State>> GetStatesAsync(int countryId);
        Task<List<City>> GetCitiesAsync(int stateId);
        Task<bool> ChangePasswordAsync(string email, string oldPassword, string newPassword);
    }
}
