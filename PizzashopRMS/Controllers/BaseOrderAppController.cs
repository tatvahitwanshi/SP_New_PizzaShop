using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Azure.Core;
using BusinessLayer.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PizzashopRMS.Helpers;

namespace PizzashopRMS.Controllers;

[CustomAuthorise(new string[] { "admin","account manager","chef" })]
public class BaseOrderAppController : Controller
{
    private string _role;
    private string _email;
    private string _imageurl;
    private ILogin _auth;
    public BaseOrderAppController()
    {
       
    }
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        _auth = context.HttpContext.RequestServices.GetService<ILogin>()!;
        base.OnActionExecuting(context);
        AllValues(context);
        
    }
    private void AllValues(ActionExecutingContext context)
    {
        var token = Request.Cookies["JWTLogin"]?.ToString();
        if (string.IsNullOrEmpty(token))
        {
            context.Result = new RedirectToActionResult("LoginView", "Login", null);
            return;

        }
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var email = jwtToken.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Name)?.Value;
        var role = jwtToken.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Role)?.Value;
        _email = email!;
        _role = role!;
        _imageurl = _auth.GetImageUrl(_email);
        if (_imageurl != null && _imageurl != "") ViewBag.image = _imageurl;
        ViewBag.email = _email;
        ViewBag.roleorderapp=_role;
    }

    public string getUserEmail()
    {
        return _email;
    }
}
