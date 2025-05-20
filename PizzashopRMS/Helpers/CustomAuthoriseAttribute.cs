using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BusinessLayer.Interface;
using DataAccessLayer.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;

namespace PizzashopRMS.Helpers;

public class CustomAuthoriseAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;
    private readonly string _requiredPermission;
    private readonly string _controllerName;

    public CustomAuthoriseAttribute(string[] roles , string requiredPermission="", string controllerName="")
    {

        _roles = roles;
        _controllerName=controllerName;
        _requiredPermission= requiredPermission;
    }
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var token = context.HttpContext.Request.Cookies["JWTLogin"];
        if (token == null)
        {
            HandleUnauthorizedRequest(context);
            return;
        }
     
        var configuration = context.HttpContext.RequestServices.GetService<IConfiguration>();
        var _auth = context.HttpContext.RequestServices.GetService<ILogin>();

        var handler = new JwtSecurityTokenHandler();
        var JWT = configuration.GetSection("JwtSettings");
        var key = Encoding.UTF8.GetBytes(JWT["Key"]);
        try
        {
            var ValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = JWT["Issuer"],
                ValidateAudience = true,
                ValidAudience = JWT["Audience"],
                ValidateLifetime = true
            };
            var ClaimsPrincipal = handler.ValidateToken(token, ValidationParameters, out SecurityToken validatedToken);
            
            var roleClaim = ClaimsPrincipal.Claims.FirstOrDefault(Claim => Claim.Type == ClaimTypes.Role)?.Value;
            if (roleClaim == null)
            {
                HandleUnauthorizedRequest(context);
                return;
            }
            int roleId = _auth.GetRoleID(roleClaim); 

            if (!_roles.Contains(roleClaim))
            { 
               HandleForbiddenRequest(context);
            }
            if(!string.IsNullOrEmpty(_requiredPermission) && !string.IsNullOrEmpty(_controllerName))
            {
                if(!_auth.HasPermission(roleId,_requiredPermission,_controllerName ))
                {
                   HandleForbiddenRequest(context);
                }
            }

        }
        catch (Exception ex)
        {
             HandleUnauthorizedRequest(context);
        }
    }
    private void HandleUnauthorizedRequest(AuthorizationFilterContext context)
    {
        if (IsAjaxRequest(context.HttpContext.Request))
        {
            // Console.WriteLine("Ajax Request");
            context.HttpContext.Response.StatusCode = 401; // Unauthorized
            context.Result = new JsonResult(new { redirectUrl = "/Login/LoginView" });
        }
        else
        {
            context.Result = new RedirectToRouteResult(new { controller = "Login", action = "LoginView" });
        }
    }
 
    private void HandleForbiddenRequest(AuthorizationFilterContext context)
    {
        if (IsAjaxRequest(context.HttpContext.Request))
        {
            context.HttpContext.Response.StatusCode = 403; // Forbidden
            context.Result = new JsonResult(new { redirectUrl = "/Login/ErrorPage" });
        }
        else
        {
            context.Result = new RedirectToRouteResult(new { controller = "Login", action = "ErrorPage" });
        }
    }
 
    private bool IsAjaxRequest(HttpRequest request)
    {
        return request.Headers["X-Requested-With"] == "XMLHttpRequest";
    }
}



