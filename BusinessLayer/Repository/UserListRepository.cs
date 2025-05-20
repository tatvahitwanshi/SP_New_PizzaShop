using System.Runtime.CompilerServices;
using BusinessLayer.Helper;
using BusinessLayer.Interface;
using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BusinessLayer.Repository;

public class UserListRepository : IUserList
{
    private readonly PizzaShopContext _db;
    private readonly IEmailService _emailService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    // Constructor to initialize dependencies
    public UserListRepository(PizzaShopContext db, IEmailService emailService, IWebHostEnvironment webHostEnvironment)
    {
        _db = db;
        _emailService = emailService;
        _webHostEnvironment = webHostEnvironment;

    }

    // Retrieves a paginated, sorted, and filtered list of users
    public async Task<PaginationParams>GetUsers(PaginationParams model, string email)
    {
        var userslist = (from user in _db.Users
                         join role in _db.Roles on user.Roleid equals role.Roleid
                         where user.Isdeleted == false && user.Email != email && (
                         user.Firstname.ToLower().Contains(model.SearchKey)

                         ) 
                         select new UserListViewModel
                         {
                             Userid = user.Userid,
                             Email = user.Email,
                             Firstname = user.Firstname,
                             Phone = user.Phone,
                             RoleName = role.Rolename,
                             Isactive = user.Isactive,
                             Profilepic = user.Profilepic,
                         });

        switch (model.sortBy)
        {
            case "name":
                userslist = (model.sortOrder == "asc") ? userslist.OrderBy(u => u.Firstname) : userslist.OrderByDescending(u => u.Firstname);
                break;
            case "role":
                userslist = (model.sortOrder == "asc") ? userslist.OrderBy(u => u.RoleName) : userslist.OrderByDescending(u => u.RoleName);
                break;
            default:
                userslist = userslist.OrderBy(u => u.Firstname);
                break;
        }

        model.Count = await userslist.CountAsync();

        if (model.PageNumber < 1)
        {
            model.PageNumber = 1;
        }

        var totalPages = (int)Math.Ceiling((double)model.Count / model.PageSize);

        if (model.PageNumber > totalPages)
        {
            model.PageNumber = totalPages;
        }

        if (model.PageNumber < 1)
        {
            model.PageNumber = 1;
        }

        model.UserList = await userslist.Skip((model.PageNumber - 1) * model.PageSize).Take(model.PageSize).ToListAsync();

        return (model);
    }

    // Retrieves list of roles
    public List<Role> GetRoles()
    {
        return _db.Roles.ToList();
    }

    // Retrieves list of countries
    public List<Country> GetCountries()
    {
        return _db.Countries.ToList();
    }

    // Retrieves states based on country ID
    public List<State> GetStatesByCountry(int countryId)
    {
        return _db.States.Where(s => s.Countryid == countryId).ToList();
    }

    // Retrieves cities based on state ID
    public List<City> GetCitiesByState(int stateId)
    {
        return _db.Cities.Where(c => c.Stateid == stateId).ToList();
    }

    // Adds a new user
    public async Task<string> AddUser(AddUserViewModel model, string email)
    {
        // Check if email already exists
        bool emailExists = await _db.Users.AnyAsync(u => u.Email == model.Email.ToLower());
        if (emailExists)
        {
            return "Email already exists";
        }
        // Check if username already exists
        bool userExists = await _db.Users.AnyAsync(u => u.Username == model.Username);
        if (userExists)
        {
            return "Username already exists";
        }
        // Check if phoneNumber already exists
        bool phoneNumber = await _db.Users.AnyAsync(u => u.Phone == model.Phone);
        if(phoneNumber){
            return "Phone Number already exists";
        }
        // Validate file extension
        // var ext = Path.GetExtension(model.Profilepic.FileName);
        // if (!ext.Equals(".jpg") && !ext.Equals(".png") && !ext.Equals(".jpeg"))
        // {
        //     return "File uploaded should be of JPG, PNG, or JPEG format only";
        // }

        // Create user object
        var user = new User
        {
            Email = model.Email.ToLower(),
            Username = model.Username,
            Password = HashingHelper.ComputeSHA256(model.Password),
            Firstname = model.Firstname.Trim(),
            Lastname = model.Lastname.Trim(),
            Profilepic = await UploadPhotoAsync(model.Profilepic),
            Zipcode = model.Zipcode,
            Address = model.Address,
            Phone = model.Phone,
            Countryid = model.Countryid,
            Stateid = model.Stateid,
            Cityid = model.Cityid,
            Roleid = model.Roleid,
            Isactive = true, // Default to active
            CreatedBy = email
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return "User added successfully";
    }

    // Sends an email for user registration
    public async Task<bool> AddUserEmail(string newEmail,string newPassword, string callbackUrl)
    {
        if (newEmail == null)
            return false;
        if(newPassword == null)
            return false;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == newEmail);
        if (user == null)
            return false;


        string subject = "Password Reset Request";
        string message = @$"
        <div style='padding: 20px; background-color: #0066A7; display: flex; justify-content: center;'>
            <h1 style='align-items: center; color:white'>PIZZASHOP</h1>
        </div>
        <div style='background-color: rgba(128, 128, 128, 0.158); padding: 3%;'>
            <span><br>Welcome Pizza shop, <br><br> 
            Please find the details below for the login into your account click <br>
            <br>
            <div style='border:solid'>
                <h2>Login Details</h2>
                <h4>Username: {newEmail}</h4>
                <h4>Temporary password:{newPassword}</h4>
            </div>
            <br>
            If you encounter any issues, please contact our support team. <br>
            
        </div>";

        return await _emailService.SendEmailAsync(user.Email, subject, message);
    }

    // Retrieves user profile details
    public async Task<EditUserViewModel> GetUserProfileDetailsAsync(int userId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Userid == userId);
        if (user == null) return null;

        return new EditUserViewModel
        {
            UserId=userId,
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
            Isactive = user.Isactive,
            Roleid = user.Roleid,
            // Profilepic = user.Profilepic


        };
    }

    // Updates user profile details
    public async Task<bool> EditUserProfileDetailsAsync(EditUserViewModel model)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (user == null) return false;

        user.Firstname = model.Firstname;
        user.Lastname = model.Lastname;
        user.Username = model.Username;
        user.Email = model.Email;
        user.Phone = model.Phone;
        user.Address = model.Address;
        user.Zipcode = model.Zipcode;
        user.Countryid = model.Countryid;
        user.Stateid = model.Stateid;
        user.Cityid = model.Cityid;
        user.Roleid = model.Roleid;
        user.Isactive = model.Isactive;
        user.EditedBy = "Admin";
        user.EditDate = DateTime.Now;
        if(model.Profilepic!=null)
        {
            user.Profilepic = await UploadPhotoAsync(model.Profilepic);
        }

        await _db.SaveChangesAsync();
        return true;
    }

    // Soft deletes a user
    public async Task DeleteUser(int userId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Userid == userId);
        if (user != null)
        {
            user.Isdeleted = true;
            user.Isactive= false;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }
    }

     // Uploads profile picture to the server
    public async Task<string?> UploadPhotoAsync(IFormFile photo)
    {
        if (photo == null || photo.Length == 0)
            return null;

        string folder = "userphoto/";
        string uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
        string serverFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder, uniqueFileName);

        using (var fileStream = new FileStream(serverFolder, FileMode.Create))
        {
            await photo.CopyToAsync(fileStream);
        }

        return "/" + folder + uniqueFileName;
    }


}
