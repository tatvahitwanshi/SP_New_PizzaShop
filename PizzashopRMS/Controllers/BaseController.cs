using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BusinessLayer.Interface;
using DataAccessLayer.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PizzashopRMS.Helpers;

namespace PizzashopRMS.Controllers;

[CustomAuthorise(new string[] { "admin","account manager" ,"chef"})]
public abstract class BaseController : Controller
{
    private string _role;
    private string _email;
    private string _imageurl;
    private string _currModule;

    public BaseController(string currModule = "")
    {
        _currModule= currModule;
    }
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        var token = Request.Cookies["JWTLogin"]?.ToString();
        if (string.IsNullOrEmpty(token))
        {
            context.Result = new RedirectToActionResult("LoginView", "Login", null);
            return;

        }

        var _auth = context.HttpContext.RequestServices.GetService<ILogin>();
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var email = jwtToken.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Name)?.Value;
        var role = jwtToken.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Role)?.Value;
        _email = email;
        _role = role;
        _imageurl = _auth.GetImageUrl(_email);
        if (_imageurl != null && _imageurl != "") ViewBag.image = _imageurl;
        ViewBag.email = _email;
        var _roleId=_auth.GetRoleID(_role);
        ViewBag.RoleNameForOrderApp=_role;

        // all about permission based view --------------------------------------------------
        Dictionary<string, bool> permissions = new Dictionary<string, bool>();
        foreach (var module in PermissionConst.GetPermissionNames())
        {
            Console.WriteLine(module+":"+_auth.HasPermission(_roleId, PermissionConst.CanView, module));
            permissions[module] = _auth.HasPermission(_roleId, PermissionConst.CanView, module);
        }
        if (_currModule != "")
        {
            permissions[PermissionConst.CanDelete] = _auth.HasPermission(_roleId, PermissionConst.CanDelete, _currModule);
            permissions[PermissionConst.CanAddEdit] = _auth.HasPermission(_roleId, PermissionConst.CanAddEdit, _currModule);
        }
        ViewBag.Permissions = permissions;
    }

}
