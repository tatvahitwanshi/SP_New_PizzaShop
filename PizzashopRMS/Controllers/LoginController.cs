using BusinessLayer.Interface;
using DataAccessLayer.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Threading.Tasks;
using DataAccessLayer.Models;
using PizzaShopApp.Helpers;


namespace PizzaShopApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogin _loginRepository;
        private readonly ILogger<LoginController> _logger;
        private readonly GenerateJwtTokenHelper _jwtTokenReader;

        // Constructor to initialize login repository and logger
        public LoginController(ILogin loginRepository, ILogger<LoginController> logger, GenerateJwtTokenHelper jwtTokenReader)
        {
            _loginRepository = loginRepository;
            _logger = logger;
            _jwtTokenReader = jwtTokenReader;

        }

        public IActionResult LoginView()
        {
            string token = Request.Cookies["JWTLogin"];
            if (!string.IsNullOrEmpty(token))
            {
                string role = _jwtTokenReader.GetRoleFromToken(token);
                // You can log it, use it, or redirect based on role
                if (role == "chef")
                {
                    return RedirectToAction("Kot", "Kot");
                }
                else
                {
                    return RedirectToAction("DashboardView", "Dashboard");
                }
            }

            return View();
        }

        // Handles user login authentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("LoginView", user);
                }

                var (dbUser, message) = await _loginRepository.AuthenticateUserAsync(user.Email, user.Password);

                if (dbUser == null)
                {
                    TempData["error"] = message;
                    return RedirectToAction("LoginView", "Login");
                }
                bool UserActivation = await _loginRepository.LoginUserActivation(user.Email);
                if (UserActivation == false)
                {
                    TempData["error"] = "User Status is Inactive";
                    return RedirectToAction("LoginView", "Login");
                }

                var role = await _loginRepository.GetRoleName(dbUser.Roleid);
                var token = await _loginRepository.GenerateJwtTokenAsync(dbUser.Email, dbUser.Roleid, Response, user.RememberMe);

                if (role == "admin")
                {
                    TempData["success"] = message;
                    return RedirectToAction("DashboardView", "Dashboard");
                }
                else if (role == "chef")
                {
                    TempData["success"] = message;
                    return RedirectToAction("Kot", "Kot");
                }
                else if (role == "account manager")
                {
                    TempData["success"] = message;
                    return RedirectToAction("DashboardView", "Dashboard");
                }
                else
                {
                    TempData["error"] = "Invalid role.";
                    return RedirectToAction("LoginView", "Login");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login.");
                throw;
            }
        }

        // Displays the forgot password page
        public IActionResult ForgotPasswordView(string email)
        {
            ViewData["email"] = string.IsNullOrEmpty(email) ? "" : email;
            return View();
        }

        // Forgot password post method
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgetPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("ForgotPasswordView", model);
                }

                bool isEmailSent = await _loginRepository.SendResetPasswordLinkAsync(model.Email, Response, Url);

                if (isEmailSent)
                {
                    TempData["success"] = "A password reset link has been sent to your email.";
                }
                else
                {
                    TempData["error"] = "Email not found!";
                }

                return View("LoginView");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ForgotPassword.");
                throw;
            }
        }


        // Displays the reset password page
        public IActionResult ResetPasswordView(string token1)
        {
            if (_loginRepository.isTokenExpired(token1))
            {

                _loginRepository.removeToken(token1);
                TempData["error"] = "Link is Expired";
                return RedirectToAction("ForgotPasswordView", "Login");
            }

            if (_loginRepository.isTokenUsed(token1))
            {
                TempData["error"] = "Link is Expired";
                return RedirectToAction("ForgotPasswordView", "Login");
            }
            string email = _loginRepository.GetEmailFromToken(token1); // Make sure this method exists in your repository

            // Pass the email to the view using ViewData
            ViewData["email"] = email;
            return View();
        }

        //Reset password post method
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            try
            {

                if (string.IsNullOrEmpty(model.Email))
                {
                    ModelState.AddModelError("", "Invalid or expired request.");
                    return View("ResetPasswordView", model);
                }

                var result = await _loginRepository.ResetPasswordAsync(model.Email, model.Password);

                if (result)
                {
                    TempData["success"] = "Password reset successfully. Please login.";
                    return RedirectToAction("LoginView");
                }
                else
                {
                    ModelState.AddModelError("", "Email not found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset.");
                throw; // Rethrow the exception
            }
            return View("ResetPasswordView", model);
        }

        // Logs out the user by clearing authentication cookies
        public IActionResult Logout()
        {
            Response.Cookies.Delete("JWTLogin");
            Response.Cookies.Delete("UserEmail");
            Response.Cookies.Delete("UserRole");
            Response.Cookies.Delete("UserName");
            return View("LoginView");
        }

        // Displays access denied page
        public IActionResult AccessDenied()
        {
            return View();
        }
        public IActionResult ErrorPage()
        {
            return View();
        }

    }
}
