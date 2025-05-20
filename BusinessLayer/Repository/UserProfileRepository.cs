using BusinessLayer.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLayer.Interface;
using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;
using Microsoft.EntityFrameworkCore;
using PizzaShopApp.Helpers;
using BusinessLayer.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace BusinessLayer.Repository;

public class UserProfileRepository : IUserProfile
{
    private readonly PizzaShopContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IUserList _userListRepository;
    
    public UserProfileRepository(PizzaShopContext db, IWebHostEnvironment webHostEnvironment,IUserList userListRepository)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
        _userListRepository=userListRepository;

    }

    // Retrieves user profile details
    public async Task<UserProfileViewModel> GetUserProfileAsync(string email)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return null;

        return new UserProfileViewModel
        {
            Email = user.Email,
            Username = user.Username,
            Phone = user.Phone,
            Firstname = user.Firstname,
            Lastname = user.Lastname,
            Address = user.Address,
            Zipcode = user.Zipcode,
            Countryid = user.Countryid,
            Stateid = user.Stateid,
            Cityid = user.Cityid,
            Profilepic = user.Profilepic,
            CountryList = await _db.Countries.ToListAsync(),
            StateList = await _db.States.Where(country => country.Countryid == user.Countryid).ToListAsync(),
            CityList = await _db.Cities.Where(state => state.Stateid == user.Stateid).ToListAsync()
        };
    }
    // public IFormFile ConvertToIFormFile(string filePath)
    // {
    //     if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
    //         return null;

    //     var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
    //     return new FormFile(stream, 0, stream.Length, "Profilepic", Path.GetFileName(filePath));
    // }

    // Updates user profile details
    public async Task<bool> UpdateUserProfileAsync(UserProfileViewModel model)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (user == null) return false;

        user.Firstname = model.Firstname.Trim();
        user.Lastname = model.Lastname.Trim();
        user.Username = model.Username;
        user.Phone = model.Phone;
        user.Address = model.Address;
        user.Zipcode = model.Zipcode;
        user.Countryid = model.Countryid;
        user.Stateid = model.Stateid;
        user.Cityid = model.Cityid;
         if (model.ImageUrl != null)
        {
            user.Profilepic = await _userListRepository.UploadPhotoAsync(model.ImageUrl);
        }

        await _db.SaveChangesAsync();
        return true;
    }

    // Retrieves states based on country ID
    public async Task<List<State>> GetStatesAsync(int countryId)
    {
        return await _db.States.Where(s => s.Countryid == countryId).ToListAsync();
    }

    // Retrieves cities based on state ID
    public async Task<List<City>> GetCitiesAsync(int stateId)
    {
        return await _db.Cities.Where(c => c.Stateid == stateId).ToListAsync();
    }

    // Changes the user's password after validation
    public async Task<bool> ChangePasswordAsync(string email, string oldPassword, string newPassword)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return false;

        var hashedOldPassword = HashingHelper.ComputeSHA256(oldPassword);
        if (user.Password != hashedOldPassword) return false;

        user.Password = HashingHelper.ComputeSHA256(newPassword);
        await _db.SaveChangesAsync();
        return true;
    }

    
}
