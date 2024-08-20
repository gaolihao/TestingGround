using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Abstractions;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Client.AspNetCore;
using MyApi.Models;

namespace MyApi.Controllers;

[ApiController]
//[Route("[controller]")]
public class AuthenticateController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly UserManager<ApplicationUser> userManager;


    public AuthenticateController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        this.signInManager = signInManager;
        this.userManager = userManager;
    }

    [HttpGet("~/callback/login/microsoft"), HttpPost("~/callback/login/microsoft")]
    public async Task<IResult> Get()
    {
        var context = HttpContext;
        var result = await context.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
        //var result = await context.AuthenticateAsync(Providers.Microsoft);

        var identity = new ClaimsIdentity(Providers.Microsoft);
        /*
        var identity = new ClaimsIdentity(
            authenticationType: "ExternalLogin",
            nameType: ClaimTypes.Name,
        roleType: ClaimTypes.Role);
        */

        var name = result.Principal!.FindFirst("Name")!.Value;
        var email = result.Principal!.GetClaim(ClaimTypes.Email) ?? "";
        var role = result.Principal!.GetClaim("roles") ?? "";
        var id = result.Principal!.GetClaim(ClaimTypes.NameIdentifier) ?? "";

        // By default, OpenIddict will automatically try to map the email/name and name identifier claims from
        // their standard OpenID Connect or provider-specific equivalent, if available. If needed, additional
        // claims can be resolved from the external identity and copied to the final authentication cookie.
        identity.SetClaim(ClaimTypes.Email, email)
                .SetClaim(ClaimTypes.Name, name)
                .SetClaim(ClaimTypes.NameIdentifier, id)
                .SetClaim(ClaimTypes.Role, role);

        //identity.AddClaim(new Claim(ClaimTypes.Role, role));

        var properties = new AuthenticationProperties
        {
            RedirectUri = result.Properties!.RedirectUri
        };

        // If needed, the tokens returned by the authorization server can be stored in the authentication cookie.
        // To make cookies less heavy, tokens that are not used are filtered out before creating the cookie.
        properties.StoreTokens(result.Properties.GetTokens().Where(token => token.Name is
            // Preserve the access and refresh tokens returned in the token response, if available.
            OpenIddictClientAspNetCoreConstants.Tokens.BackchannelAccessToken or
            OpenIddictClientAspNetCoreConstants.Tokens.RefreshToken));

        // For scenarios where the default sign-in handler configured in the ASP.NET Core
        // authentication options shouldn't be used, a specific scheme can be specified here.
        return Results.SignIn(new ClaimsPrincipal(identity), properties);
    }

    

}