// using System.IdentityModel.Tokens.Jwt;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BusinessLayer.Interface;
using DataAccessLayer.Constants;
using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;
using Microsoft.AspNetCore.Mvc;
using PizzashopRMS.Helpers;

namespace PizzashopRMS.Controllers;

[CustomAuthorise(new string[] { "admin","account manager"})]
public class UserListController : BaseController
{
    private readonly IUserList _userListRepository;
    private readonly ILogger<UserListController> _logger;

    public UserListController(IUserList userListRepository, ILogger<UserListController> logger):base(PermissionConst.USERS)
    {
        _userListRepository = userListRepository;
        _logger = logger;

    }

    // Retrieves user list with pagination, sorting, and search functionality
    [HttpGet]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> UserListView(PaginationParams model)
    {
        try
        {
            var token = Request.Cookies["JWTLogin"];
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var email = jwtToken.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Name)?.Value ?? "";

            // Get data from repository (returns a tuple)
            PaginationParams returnModel = await _userListRepository.GetUsers(model, email);
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_PartialUserList", returnModel); // Return partial view for AJAX
            }
            // Pass only the user list (List<UserListViewModel>) to the View
            return View(returnModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user list.");
            TempData["error"] = "An error occurred while retrieving the user list.";
            return RedirectToAction("DashboardView", "Dashboard");
        }

    }

    // Returns the view for adding a new user
    [HttpGet]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult AddUserView()
    {
       
        var model = new AddUserViewModel
        {
            Roles = _userListRepository.GetRoles(),
            Countries = _userListRepository.GetCountries(),
            States = new List<State>(),
            Cities = new List<City>()
        };

        return View(model);
  
    }

    // Fetch states based on selected country
    [HttpGet]
    public JsonResult GetStates(int countryId)
    {
        var states = _userListRepository.GetStatesByCountry(countryId);
        return Json(states);
    }

    // Fetch cities based on selected state
    [HttpGet]
    public JsonResult GetCities(int stateId)
    {
        var cities = _userListRepository.GetCitiesByState(stateId);
        return Json(cities);
    }

    // Retrieves user email from JWT token stored in cookies
    private string GetUserEmailFromToken()
    {
        try
        {
            var token = Request.Cookies["JWTLogin"];
            if (string.IsNullOrEmpty(token)) return "";

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Name)?.Value ?? "";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading JWT token.");
            return "";
        }
    }

    // Adds a new user to the system and sends an email notification
    [HttpPost]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> AddUserViewAsync(AddUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        try
        {
            string email = GetUserEmailFromToken();
            string resultMessage = await _userListRepository.AddUser(model, email);

            TempData["message"] = resultMessage; // Store the message for toast notification

            if (resultMessage != "User added successfully")
            {
                TempData["error"] = resultMessage;
                return RedirectToAction("UserListView");
            }

            string callbackUrl = Url.ActionLink("UserListView", "UserList");
            string newEmail = model.Email!;
            string newPassword=model.Password;
            bool isEmailSent = await _userListRepository.AddUserEmail(newEmail,newPassword, callbackUrl);

            if (isEmailSent)
            {
                TempData["success"] = "New user is added successfully, and an email has been sent!";
            }
            else
            {
                TempData["error"] = "Email could not be sent!";
            }

            return RedirectToAction("UserListView");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while adding user.");
            TempData["error"] = "An error occurred while adding the user.";
            return RedirectToAction("UserListView");
        }


    }

    // Retrieves the details of a user for editing
    [HttpGet]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> EditUserView(int userId)
    {
        try
        {
            var model = await _userListRepository.GetUserProfileDetailsAsync(userId);
            if (model == null) return NotFound("User Not Found");
            model.Roles = _userListRepository.GetRoles();
            model.Countries = _userListRepository.GetCountries();
            model.States = _userListRepository.GetStatesByCountry(model.Countryid);
            model.Cities = _userListRepository.GetCitiesByState(model.Stateid);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user details.");
            TempData["error"] = "An error occurred while retrieving user details.";
            return RedirectToAction("UserListView");
        }

    }

    // Updates user profile details
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> EditUserProfileView(EditUserViewModel model)
    {
        // Thread.Sleep(1000);

        if (!ModelState.IsValid)
        {
            TempData["error"] = "Enter data was not proper";
            return RedirectToAction("UserListView");
        }
        try
        {
            var success = await _userListRepository.EditUserProfileDetailsAsync(model);
            if (success)
            {
                TempData["success"] = "User Updated Successfully";
                return RedirectToAction("UserListView");
            }
            else
            {
                TempData["Error"] = "Failed to update user.";
                ModelState.AddModelError("", "Failed to update user.");
                return View("UserListView", model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile.");
            TempData["error"] = "An error occurred while updating the user profile.";
            return RedirectToAction("UserListView", model);
        }

    }

    // Deletes a user 
    [HttpPost]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        try
        {
            await _userListRepository.DeleteUser(userId);
            TempData["success"] = "User deleted successfully.";

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user.");
            TempData["error"] = "An error occurred while deleting the user.";
        }

        return RedirectToAction("UserListView");

    }

}