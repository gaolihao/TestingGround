using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using static OpenIddict.Abstractions.OpenIddictConstants;
using OpenIddict.Server.AspNetCore;
using Microsoft.AspNetCore.Http.Extensions;
using OpenIddict.Abstractions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Identity;
using MyApi.Models;

namespace MyApi.Controllers;

[ApiController]
public class AuthorizeController : ControllerBase
{
    //private readonly ILogger<AuthorizeController> logger = logger;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;

    public AuthorizeController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
    }


    [HttpGet("~/connect/authorize"), HttpPost("~/connect/authorize")]
    public async Task<IResult> Get()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var context = HttpContext;

        // Important: when using ASP.NET Core Identity and its default UI, the identity created in this action is
        // not directly persisted in the final authentication cookie (called "application cookie" by Identity) but
        // in an intermediate authentication cookie called "external cookie" (the final authentication cookie is
        // later created by Identity's ExternalLogin Razor Page by calling SignInManager.ExternalLoginSignInAsync()).
        //
        // Unfortunately, this process doesn't preserve the claims added here, which prevents flowing claims
        // returned by the external provider down to the final authentication cookie. For scenarios that
        // require that, the claims can be stored in Identity's database by calling UserManager.AddClaimAsync()
        // directly in this action or by scaffolding the ExternalLogin.cshtml page that is part of the default UI:
        // https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/additional-claims#add-and-update-user-claims.
        //
        // Alternatively, if flowing the claims from the "external cookie" to the "application cookie" is preferred,
        // the default ExternalLogin.cshtml page provided by Identity can be scaffolded to replace the call to
        // SignInManager.ExternalLoginSignInAsync() by a manual sign-in operation that will preserve the claims.
        // For scenarios where scaffolding the ExternalLogin.cshtml page is not convenient, a custom SignInManager
        // with an overridden SignInOrTwoFactorAsync() method can also be used to tweak the default Identity logic.
        //
        // For more information, see https://haacked.com/archive/2019/07/16/external-claims/ and
        // https://stackoverflow.com/questions/42660568/asp-net-core-identity-extract-and-save-external-login-tokens-and-add-claims-to-l/42670559#42670559.
        var result = (await context.AuthenticateAsync(IdentityConstants.ExternalScheme));
        var principal = result?.Principal;
        if (principal is null)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = context.Request.GetEncodedUrl()
            };

            var r = Results.Challenge(properties, [Providers.Microsoft]);
            return r;
        }

        var name = principal.FindFirst(ClaimTypes.Name)!.Value;
        //var email = principal.FindFirst(ClaimTypes.Email)!.Value;
        var identifier = principal.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var role = principal.FindFirst(ClaimTypes.Role)!.Value;

        /*
        var appUser = new ApplicationUser()
        {
            UserName = identifier,
            Email = email,
            Id = identifier,
            NormalizedUserName = name,
        };

        // By default, Identity requires that passwords contain an uppercase character, lowercase character, a digit, and a non-alphanumeric character. Passwords must be at least six characters long.
        var password = "Ss1---";
        await EnsureUserAsync(appUser, password, [role]);
        var newUser = await userManager.FindByEmailAsync(appUser.Email!);
        */

        //await userManager.AddToRoleAsync(newUser!, role);

        // Create the claims-based identity that will be used by OpenIddict to generate tokens.
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        // Import a few select claims from the identity stored in the local cookie.
        identity.AddClaim(new Claim(Claims.Subject, identifier));
        identity.AddClaim(new Claim(Claims.Name, name).SetDestinations(Destinations.AccessToken));
        identity.AddClaim(new Claim(Claims.Email, "").SetDestinations(Destinations.AccessToken));
        identity.AddClaim(new Claim(Claims.Role, role).SetDestinations(Destinations.AccessToken));

        return Results.SignIn(new ClaimsPrincipal(identity), properties: null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /*
    async Task EnsureUserAsync(ApplicationUser user, string password, string[]? roles = null)
    {
        var validPass = (await userManager.PasswordValidators[0].ValidateAsync(userManager, user, password).ConfigureAwait(false)).Succeeded;
        if (!validPass)
        {
            return;
        }

        var existingUser = await userManager.FindByEmailAsync(user.Email!);
        if (existingUser != null) return;

        await userManager!.CreateAsync(user, password);
        
        if (roles?.Length > 0)
        {
            var newUser = await userManager.FindByEmailAsync(user.Email!);
            await userManager.AddToRoleAsync(user, roles[0]);
        }
    }
    */

    [HttpPost("~/connect/token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {

        var request = HttpContext.GetOpenIddictServerRequest();

        // Retrieve the claims principal stored in the authorization code/refresh token.
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        var principal = result?.Principal!;
        var name = principal.FindFirst("Name")!.Value;
        var email = principal.FindFirst("email")!.Value;
        var role = principal.FindFirst("role")!.Value;
        var id = principal.FindFirst(Claims.Subject)!.Value;

        //var u = userManager.FindByNameAsync(n2);
        //var user = await userManager.FindByEmailAsync(e2);

        var identity = new ClaimsIdentity(principal.Claims,
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        identity.SetClaim(Claims.Subject, id)
                .SetClaim(Claims.Email, email)
                .SetClaim(Claims.Name, name)
                .SetClaims(Claims.Role, [role]);

        identity.SetDestinations(GetDestinations);

        // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        throw new NotImplementedException("The specified grant type is not implemented.");

    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        // Note: by default, claims are NOT automatically included in the access and identity tokens.
        // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        // whether they should be included in access tokens, in identity tokens or in both.

        switch (claim.Type)
        {
            case Claims.Name or Claims.PreferredUsername:
                yield return Destinations.AccessToken;

                if (claim.Subject!.HasScope(Scopes.Profile))
                    yield return Destinations.IdentityToken;

                yield break;

            case Claims.Email:
                yield return Destinations.AccessToken;

                if (claim.Subject!.HasScope(Scopes.Email))
                    yield return Destinations.IdentityToken;

                yield break;

            case Claims.Role:
                yield return Destinations.AccessToken;

                if (claim.Subject!.HasScope(Scopes.Roles))
                    yield return Destinations.IdentityToken;

                yield break;

            // Never include the security stamp in the access and identity tokens, as it's a secret value.
            case "AspNet.Identity.SecurityStamp": yield break;

            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }
}