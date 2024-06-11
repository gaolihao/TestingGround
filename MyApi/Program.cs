using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using Quartz;
using MyApi;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddDbContext<TodoContext>(opt => opt.UseInMemoryDatabase("Todos"));
//builder.Services.AddDatabaseDeveloperPageExceptionFilter();
//builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/*
builder.Services.AddOpenIddict()

            // Register the OpenIddict client components.
            .AddClient(options =>
            {
                // Note: this sample uses the code flow, but you can enable the other flows if necessary.
                options.AllowAuthorizationCodeFlow();

                // Register the signing and encryption credentials used to protect
                // sensitive data like the state tokens produced by OpenIddict.
                options.AddDevelopmentEncryptionCertificate()
                       .AddDevelopmentSigningCertificate();

                // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                options.UseAspNetCore()
                       .EnableStatusCodePagesIntegration()
                       .EnableRedirectionEndpointPassthrough();

                // Register the System.Net.Http integration and use the identity of the current
                // assembly as a more specific user agent, which can be useful when dealing with
                // providers that use the user agent as a way to throttle requests (e.g Reddit).
                options.UseSystemNetHttp()
                       .SetProductInformation(typeof(Startup).Assembly);

                // Register the Web providers integrations.
                //
                // Note: to mitigate mix-up attacks, it's recommended to use a unique redirection endpoint
                // URI per provider, unless all the registered providers support returning a special "iss"
                // parameter containing their URL as part of authorization responses. For more information,
                // see https://datatracker.ietf.org/doc/html/draft-ietf-oauth-security-topics#section-4.4.
                options.UseWebProviders()
                       .AddGitHub(options =>
                       {
                           options.SetClientId("c4ade52327b01ddacff3")
                                  .SetClientSecret("da6bed851b75e317bf6b2cb67013679d9467c122")
                                  .SetRedirectUri("callback/login/github");
                       });
            });
*/

// OpenIddict offers native integration with Quartz.NET to perform scheduled tasks
// (like pruning orphaned authorizations/tokens from the database) at regular intervals.
builder.Services.AddQuartz(options =>
{
    options.UseMicrosoftDependencyInjectionJobFactory();
    options.UseSimpleTypeLoader();
    options.UseInMemoryStore();
});

// Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

builder.Services.AddDbContext<DbContext>(options =>
{
    options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "openiddict-mimban-server.sqlite3")}");
    options.UseOpenIddict();
});

builder.Services.AddOpenIddict()

    // Register the OpenIddict core components.
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<DbContext>();
    })

    // Register the OpenIddict client components.
    .AddClient(options =>
    {
        // Note: this sample uses the code flow, but you can enable the other flows if necessary.
        options.AllowAuthorizationCodeFlow();

        // Register the signing and encryption credentials used to protect
        // sensitive data like the state tokens produced by OpenIddict.
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        options.UseAspNetCore()
               .EnableRedirectionEndpointPassthrough();

        // Register the System.Net.Http integration and use the identity of the current
        // assembly as a more specific user agent, which can be useful when dealing with
        // providers that use the user agent as a way to throttle requests (e.g Reddit).
        options.UseSystemNetHttp()
               .SetProductInformation(typeof(Program).Assembly);

        // Register the Web providers integrations.
        //
        // Note: to mitigate mix-up attacks, it's recommended to use a unique redirection endpoint
        // URI per provider, unless all the registered providers support returning a special "iss"
        // parameter containing their URL as part of authorization responses. For more information,
        // see https://datatracker.ietf.org/doc/html/draft-ietf-oauth-security-topics#section-4.4.
        options.UseWebProviders()
               .AddGitHub(options =>
               {
                   options.SetClientId("c4ade52327b01ddacff3")
                          .SetClientSecret("da6bed851b75e317bf6b2cb67013679d9467c122")
                          .SetRedirectUri("callback/login/github");
               });
    })

    // Register the OpenIddict server components.
    .AddServer(options =>
    {
        // Enable the authorization and token endpoints.
        options.SetAuthorizationEndpointUris("authorize")
               .SetTokenEndpointUris("token");

        // Note: this sample only uses the authorization code flow but you can enable
        // the other flows if you need to support implicit, password or client credentials.
        options.AllowAuthorizationCodeFlow();

        // Register the signing and encryption credentials.
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        //
        // Note: unlike other samples, this sample doesn't use token endpoint pass-through
        // to handle token requests in a custom MVC action. As such, the token requests
        // will be automatically handled by OpenIddict, that will reuse the identity
        // resolved from the authorization code to produce access and identity tokens.
        //
        options.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough();
    })

    // Register the OpenIddict validation components.
    .AddValidation(options =>
    {
        // Import the configuration from the local OpenIddict server instance.
        options.UseLocalServer();

        // Register the ASP.NET Core host.
        options.UseAspNetCore();
    });

builder.Services.AddAuthorization()
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

var app = builder.Build();

// app.UseHttpsRedirection();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

/*
// Create a new application registration matching the values configured in Mimban.Client.
// Note: in a real world application, this step should be part of a setup script.
await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DbContext>();
    await context.Database.EnsureCreatedAsync();

    var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

    if (await manager.FindByClientIdAsync("console_app") == null)
    {
        await manager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ApplicationType = ApplicationTypes.Native,
            ClientId = "console_app",
            ClientType = ClientTypes.Public,
            RedirectUris =
            {
                new Uri("http://localhost/")
            },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.ResponseTypes.Code
            }
        });
    }
}
*/

// Create a new application registration matching the values configured in Mimban.Client.
// Note: in a real world application, this step should be part of a setup script.
await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DbContext>();
    await context.Database.EnsureCreatedAsync();

    var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

    if (await manager.FindByClientIdAsync("console_app") == null)
    {
        await manager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ApplicationType = ApplicationTypes.Native,
            ClientId = "console_app",
            ClientType = ClientTypes.Public,
            RedirectUris =
            {
                new Uri("http://localhost/")
            },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.ResponseTypes.Code
            }
        });
    }
}

app.UseAuthentication();
app.UseAuthorization();

/*
app.MapGet("api", [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
(ClaimsPrincipal user) => user.Identity!.Name);



app.MapMethods("callback/login/github", [HttpMethods.Get, HttpMethods.Post], async (HttpContext context) =>
{
    // Resolve the claims extracted by OpenIddict from the userinfo response returned by GitHub.
    var result = await context.AuthenticateAsync(Providers.GitHub);

    var identity = new ClaimsIdentity(
        authenticationType: "ExternalLogin",
        nameType: ClaimTypes.Name,
        roleType: ClaimTypes.Role);

    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, result.Principal!.FindFirst("id")!.Value));

    var properties = new AuthenticationProperties
    {
        RedirectUri = result.Properties!.RedirectUri
    };

    // For scenarios where the default sign-in handler configured in the ASP.NET Core
    // authentication options shouldn't be used, a specific scheme can be specified here.
    return Results.SignIn(new ClaimsPrincipal(identity), properties);
});

app.MapMethods("authorize", [HttpMethods.Get, HttpMethods.Post], async (HttpContext context) =>
{
    // Resolve the claims stored in the cookie created after the GitHub authentication dance.
    // If the principal cannot be found, trigger a new challenge to redirect the user to GitHub.
    //
    // For scenarios where the default authentication handler configured in the ASP.NET Core
    // authentication options shouldn't be used, a specific scheme can be specified here.
    var principal = (await context.AuthenticateAsync())?.Principal;
    if (principal is null)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = context.Request.GetEncodedUrl()
        };

        return Results.Challenge(properties, [Providers.GitHub]);
    }

    var identifier = principal.FindFirst(ClaimTypes.NameIdentifier)!.Value;

    // Create the claims-based identity that will be used by OpenIddict to generate tokens.
    var identity = new ClaimsIdentity(
        authenticationType: TokenValidationParameters.DefaultAuthenticationType,
        nameType: Claims.Name,
        roleType: Claims.Role);

    // Import a few select claims from the identity stored in the local cookie.
    identity.AddClaim(new Claim(Claims.Subject, identifier));
    identity.AddClaim(new Claim(Claims.Name, identifier).SetDestinations(Destinations.AccessToken));
    identity.AddClaim(new Claim(Claims.PreferredUsername, identifier).SetDestinations(Destinations.AccessToken));

    return Results.SignIn(new ClaimsPrincipal(identity), properties: null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
});
*/


app.MapControllers();

app.Run();



//var app = builder.Build();

// Configure the HTTP request pipeline.
/*
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}
*/

//app.Run();

/*
app.MapGet("/todoitems", async (TodoDb db) =>
    await db.Todos.ToListAsync());

app.MapGet("/todoitems/complete", async (TodoDb db) =>
    await db.Todos.Where(t => t.IsComplete).ToListAsync());

app.MapGet("/todoitems/{id}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id)
        is Todo todo
            ? Results.Ok(todo)
            : Results.NotFound());

app.MapPost("/todoitems", async (Todo todo, TodoDb db) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todoitems/{todo.Id}", todo);
});

app.MapPut("/todoitems/{id}", async (int id, Todo inputTodo, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});
*/

/*
app.UseAuthorization();

app.MapControllers();

app.Run();

*/
