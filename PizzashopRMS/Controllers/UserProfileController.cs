using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BusinessLayer.Interface;
using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;
using Microsoft.AspNetCore.Mvc;
using PizzashopRMS.Controllers;
using PizzashopRMS.Helpers;

namespace PizzaShopApp.Controllers
{
    [CustomAuthorise(new string[] { "admin","account manager" })]
    public class UserProfileController : BaseController
    {
        private readonly IUserProfile _userProfile;
        private readonly ILogger<UserProfileController> _logger;

        public UserProfileController(IUserProfile userProfile, ILogger<UserProfileController> logger) : base()
        {
            _userProfile = userProfile;
            _logger = logger;

        }

        // Extracts the user's email from the JWT token stored in cookies
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
                _logger.LogError(ex, "Error extracting email from token");
                return ""; // Return empty string if token is invalid or missing
            }

        }

        // User Profile view get method
        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> UserProfileView()
        {
            try
            {
                var token = Request.Cookies["JWTLogin"];
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var role = jwtToken.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Role)?.Value ?? "";
                ViewData["Role"] = role;
                string email = GetUserEmailFromToken();
                if (string.IsNullOrEmpty(email)) return RedirectToAction("LoginView", "Login");

                var model = await _userProfile.GetUserProfileAsync(email);
                if (model == null) return NotFound("User Not Found");

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user profile");
                TempData["error"] = "An error occurred while loading the profile.";
                return RedirectToAction("LoginView", "Login");
            }


        }

        //User profile post method to save the updated profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> UserProfileView(UserProfileViewModel model)
        {
            try
            {
                if (model.Username == null || model.Countryid == 0 || model.Cityid == 0 || model.Stateid == 0)
                {
                    if (model.Countryid == 0)
                    {
                        TempData["error"] = "Please Select the country";
                        return RedirectToAction("UserProfileView");
                    }
                    if (model.Stateid == 0)
                    {
                        TempData["error"] = "Please Select the state";
                        return RedirectToAction("UserProfileView");

                    }
                    if (model.Stateid == 0)
                    {
                        TempData["error"] = "Please Select the city";
                        return RedirectToAction("UserProfileView");

                    }

                }
                var success = await _userProfile.UpdateUserProfileAsync(model);
                if (success)
                {
                    TempData["success"] = "User Updated successfully";
                    return RedirectToAction("UserProfileView");
                }
                else
                {
                    TempData["error"] = "User Updated successfully";
                    return RedirectToAction("UserProfileView");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                TempData["error"] = "An error occurred while updating the profile.";
                return RedirectToAction("UserProfileView");
            }

        }

        // Fetch states based on selected country
        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> GetStates(int countryId)
        {
            try
            {
                var states = await _userProfile.GetStatesAsync(countryId);
                return Json(states.Select(s => new { stateId = s.Stateid, stateName = s.Statename }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading states for CountryId: {countryId}");
                return Json(new { error = "Failed to load states" });
            }
        }

        // Fetch city based on selected state
        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> GetCities(int stateId)
        {
            try
            {
                var cities = await _userProfile.GetCitiesAsync(stateId);
                return Json(cities.Select(c => new { cityId = c.Cityid, cityName = c.Cityname }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading cities for StateId: {stateId}");
                return Json(new { error = "Failed to load cities" });
            }
        }

        // Change password view
        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult ChangePasswordView()
        {
            return View();
        }

        //Change password post method 
        [HttpPost]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["error"] = "Enter the password in proper formate";
                    return View("ChangePasswordView");
                }

                string email = GetUserEmailFromToken();
                var success = await _userProfile.ChangePasswordAsync(email, model.OldPassword, model.ConfirmNewPassword);

                if (success)
                {
                    TempData["success"] = "Password changed successful";
                    return RedirectToAction("LoginView", "Login");

                }
                else
                {
                    TempData["error"] = "Old Password does not match";
                    return View("ChangePasswordView");

                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                TempData["error"] = "An error occurred while changing the password.";
                return View("ChangePasswordView");
            }

        }
    }
}
