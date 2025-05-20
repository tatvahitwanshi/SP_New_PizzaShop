using System;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using BusinessLayer.Helper;
using BusinessLayer.Interface;
using DataAccessLayer.Constants;
using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaShopApp.Helpers;

namespace BusinessLayer.Repository;

public class LoginRepository : ILogin
{
    private readonly PizzaShopContext _db;
    private readonly GenerateJwtTokenHelper _jwtTokenHelper;
    private readonly IEmailService _emailService; // Assuming you have an EmailService

    // Constructor to initialize dependencies
    public LoginRepository(PizzaShopContext db, GenerateJwtTokenHelper jwtTokenHelper, IEmailService emailService)
    {
        _db = db;
        _jwtTokenHelper = jwtTokenHelper;
        _emailService = emailService;

    }
    public string GetImageUrl(string email)
    {
        return _db.Users.Where(a => a.Email == email).Select(a => a.Profilepic).FirstOrDefault() ?? "";
    }

    public bool HasPermission(int roleid, string WhichPermission, string PermissionName)
    {
        return _db.Permissions
            .Join(_db.Permissionlists,
                permissionTable => permissionTable.Permissionid,
                permissionlistsTable => permissionlistsTable.Permissionid,
                (permissionTable, permissionlistsTable) => new { permissionTable, permissionlistsTable })
            .Where(x => x.permissionTable.Roleid == roleid
                    && x.permissionlistsTable.Permissionname == PermissionName)
            .Any(p => (WhichPermission == PermissionConst.CanView && (p.permissionTable.Canview ?? false))
                   || (WhichPermission == PermissionConst.CanAddEdit && (p.permissionTable.Canaddedit ?? false))
                   || (WhichPermission == PermissionConst.CanDelete && (p.permissionTable.Candelete ?? false)));
    }

    // Authenticates user by verifying email and password
    public async Task<(User, string)> AuthenticateUserAsync(string email, string password)
    {
        var user = await Task.Run(() => _db.Users.FirstOrDefault(u => u.Email == email.ToLower()));
        if (user == null) return (null, "User Does not exist");

        var hashedPassword = HashingHelper.ComputeSHA256(password);
        return user.Password == hashedPassword ? (user, "Login Successfull") : (null, "Wrong password");
    }
    public async Task<bool> LoginUserActivation(string email)
    {
        var Email = await _db.Users.FirstOrDefaultAsync(e => e.Email == email.ToLower() && e.Isactive == true);
        if (Email == null)
        {
            return false;
        }
        else
        {
            return true;
        }

    }

    public int GetRoleID(string Rolename)
    {
        var role = _db.Roles.FirstOrDefault(r => r.Rolename == Rolename);
        return role.Roleid;
    } 
    //Get Rolename from RoleId
    public async Task<string> GetRoleName(int roleId)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Roleid == roleId);
        return role.Rolename;
    }

    // Generates a JWT token and sets it in HTTP cookies
    public async Task<string> GenerateJwtTokenAsync(string email, int roleId, HttpResponse response, bool rememberMe)
    {

        var userRole = _db.Roles.FirstOrDefault(r => r.Roleid == roleId);
        if (userRole == null) return null;

        int expiryInMinutes = rememberMe ? 24*60 : 60; // Set token expiry based on 'remember me' preference
       

        var token = _jwtTokenHelper.GenerateJwtToken(email, userRole.Rolename, expiryInMinutes);

        // Sets JWT token in cookies based on 'remember me' preference
        response.Cookies.Append("JWTLogin", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(expiryInMinutes)
        });
        return token;


    }
     public async Task<bool> SendResetPasswordLinkAsync(string email, HttpResponse response, IUrlHelper urlHelper)
    {
        // Check if user exists first
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return false;

        // Generate token
        var token = _jwtTokenHelper.GenerateJwtToken(email, "", 60);

        // Store token in DB
        await _db.Resettokens.AddAsync(new Resettoken { Token = token });
        await _db.SaveChangesAsync();

        // Generate callback URL
        var callbackUrl = urlHelper.ActionLink("ResetPasswordView", "Login", new { token1 = token });

        // Compose email message
        string subject = "Password Reset Request";
        string message = @$"
            <div style='padding: 20px; background-color: #0066A7; display: flex; justify-content: center;'>
                <h1 style='align-items: center; color:white'>PIZZASHOP</h1>
            </div>
            <div style='background-color: rgba(128, 128, 128, 0.158); padding: 3%;'>
                <span><br> Pizza shop, <br><br> 
                Please click <a href='{callbackUrl}'>here</a> to reset your password. <br><br>
                If you encounter any issues, please contact our support team. <br>
                <span style='color: yellow;'>Important Note:</span> This link will expire in 24 hours. If you did not request a password reset, ignore this email. <br><br>    
                </span>
            </div>";

        // Send email
        return await _emailService.SendEmailAsync(user.Email, subject, message);
    }

    // Generates a reset email token and stores it in HTTP cookies
    // public async Task<string> ResetEmailToken(string email, int roleId, HttpResponse response, bool rememberMe)
    // {
    //     var token1 = _jwtTokenHelper.GenerateJwtToken(email, "", 60);
    //     storeToken(token1);
    //     return token1;


    // }
   

    // Resets user password after verification
    public async Task<bool> ResetPasswordAsync(string email, string newPassword)
    {
        try
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return false;
            }

            user.Password = HashingHelper.ComputeSHA256(newPassword); // Hash password
            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    // public async Task storeToken(string emailToken)
    // {
    //     await _db.Resettokens.AddAsync(new Resettoken{
    //         Token = emailToken,
    //     });
    //     await _db.SaveChangesAsync();
    // }

    public bool isTokenExpired(string Token)
    {
        var handler = new JwtSecurityTokenHandler();
        if(Token==null){
            return true;
        }
        var jwtToken = handler.ReadJwtToken(Token);

        if(jwtToken.ValidTo < DateTime.UtcNow){
            return true;
        }
        return false;
    }
    public void removeToken(string token)
    {
        IQueryable<Resettoken> removeRange = _db.Resettokens.Where(e=>e.Token == token || e.Createdtime < DateTime.Now.AddHours(-24));
        _db.Resettokens.RemoveRange(removeRange.ToList());

        _db.SaveChanges();
    }
    public bool isTokenUsed(string token)
    {
        bool answer = false;
        Resettoken? obj = _db.Resettokens.FirstOrDefault(e => e.Token == token);

        if(obj == null) answer = true;
        else removeToken(token);

        return answer;
    }
    public string GetEmailFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "email" || c.Type == ClaimTypes.Name);

        return emailClaim?.Value;
    }

   
}
