﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

public record Username(string value);

public class UserNameController : Controller
{
    [HttpGet("username")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public Task<ActionResult<Username>> GetUserName()
    {
        var userName = User.Identity!.Name;
        return new userName;
    }
}