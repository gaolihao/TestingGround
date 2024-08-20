using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;
namespace MyApi.Controllers;


public record Username(string value);

[ApiController]
//[Route("[controller]")]
//[Authorize(Roles = "Signin2,Signin,Writers")]
[Route("userinfo")]
public class UserNameController : Controller
{
    [HttpGet("usernameid")]
    //[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public Task<ActionResult<string>> GetUserName()
    {
        var identity = (ClaimsIdentity)User.Identity!;
        //var userName = identity.Name;
        //var claims = identity.Claims;
        var userNameId = identity.FindFirst(Claims.Subject)!.Value;

        if (string.IsNullOrEmpty(userNameId))
        {
            return Task.FromResult<ActionResult<string>>(("User name not found."));
        }
        return Task.FromResult<ActionResult<string>>((userNameId ?? ""));

    }


    [HttpGet("username")]
    //[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public Task<ActionResult<int>> GetUserNameId()
    {
        var identity = (ClaimsIdentity)User.Identity!;
        var claims = User.Claims;
        //var userName = identity.FindFirst("Name")!.Value;
        var userName = 2;

        //if (string.IsNullOrEmpty(userName))
        //{
        //    return Task.FromResult<ActionResult<string>>(("User name not found."));
        //}
        return Task.FromResult<ActionResult<int>>((userName));

    }

    [HttpGet("email")]
    //[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public Task<ActionResult<string>> GetEmail()
    {
        var identity = (ClaimsIdentity)User.Identity!;
        var email = identity.FindFirst(Claims.Email)!.Value;

        if (string.IsNullOrEmpty(email))
        {
            return Task.FromResult<ActionResult<string>>(("User name not found."));
        }
        return Task.FromResult<ActionResult<string>>((email ?? ""));

    }
}