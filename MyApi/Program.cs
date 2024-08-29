
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using Quartz;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Microsoft.AspNetCore.Identity;
using MyApi.Data;
using MyApi.Models;
using MyApi.Hubs;
using System.Text.Json;
using MyApi.Services;
using ProtoBuf.Grpc.Server;
using MyApi;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Configure the context to use sqlite.
    options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "openiddict-lalala20-server.sqlite3")}");

    // Register the entity sets needed by OpenIddict.
    // Note: use the generic overload if you need
    // to replace the default OpenIddict entities.
    options.UseOpenIddict();
});

// Register the Identity services.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// This replaces the default `SignInManager` with my custom one.
//builder.Services.AddScoped<SignInManager<ApplicationUser>, ExternalClaimsSignInManager<ApplicationUser>>();

/*
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<DbContext>();
*/

//            .AddDefaultTokenProviders();


/*
services.AddIdentity<IdentityUser, Role>().AddEntityFrameworkStores<Context>();
*/

// OpenIddict offers native integration with Quartz.NET to perform scheduled tasks
// (like pruning orphaned authorizations/tokens from the database) at regular intervals.
builder.Services.AddQuartz(options =>
{
    options.UseMicrosoftDependencyInjectionJobFactory();
    options.UseSimpleTypeLoader();
    options.UseInMemoryStore();
});

builder.Services.AddControllers();

// Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

/*
builder.Services.AddDbContext<DbContext>(options =>
{
    options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "openiddict-mimban-server.sqlite3")}");
    options.UseOpenIddict();
});
*/

var secret = Environment.GetEnvironmentVariable("CLIENT_SECRET");

builder.Services.AddOpenIddict()

    // Register the OpenIddict core components.
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore().UseDbContext<ApplicationDbContext>();
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
        // parameter containing their URL as part of authorization responses. For more information, .SetAuthority("https://login.microsoftonline.com/zomp")
        // see https://datatracker.ietf.org/doc/html/draft-ietf-oauth-security-topics#section-4.4.
        
        options.UseWebProviders()
               .AddMicrosoft(options =>
               {
                   options.SetClientId("776890c9-6376-42df-9fa3-1393af84e01b")
                          .SetClientSecret(secret)
                          .SetRedirectUri("callback/login/microsoft");
                          //.SetTenant("consumers"
               });
    })

    // Register the OpenIddict server components.
    .AddServer(options =>
    {
        // Enable the authorization and token endpoints.
        options.SetAuthorizationEndpointUris("connect/authorize")
               .SetTokenEndpointUris("connect/token");

        // Note: this sample only uses the authorization code flow but you can enable
        // the other flows if you need to support implicit, password or client credentials.
        options.AllowAuthorizationCodeFlow();

        // Register the signing and encryption credentials.
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        //options.SetTokenEndpointUris("/connect/token");

        // Enable the password flow.
        options.AllowPasswordFlow();

        // Accept anonymous clients (i.e clients that don't send a client_id).
        options.AcceptAnonymousClients();

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        //
        // Note: unlike other samples, this sample doesn't use token endpoint pass-through
        // to handle token requests in a custom MVC action. As such, the token requests
        // will be automatically handled by OpenIddict, that will reuse the identity
        // resolved from the authorization code to produce access and identity tokens.
        //
        options.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough()
               .DisableTransportSecurityRequirement()
               .EnableTokenEndpointPassthrough();
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

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

//builder.WebHost.ConfigureKestrel(SocketSetup.Execute);
builder.Services.AddGrpc();
builder.Services.AddCodeFirstGrpc();
builder.Services.AddSingleton<SynchronizedFeatureListRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

//app.UseHttpsRedirection();

// Create a new application registration matching the values configured in Mimban.Client.
// Note: in a real world application, this step should be part of a setup script.
await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
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
                Permissions.ResponseTypes.Code,
            }
        });
    }
}

app.UseRouting();
app.MapGrpcService<SynchronizedFeatureListService>();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();


JsonWriterOptions _jsonWriterOptions = new JsonWriterOptions { Indented = true };

//app.MapHub<ChatHub>("/default");
//app.MapConnectionHandler<MessagesConnectionHandler>("/chat");

/*
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<Chat>("/default");

    endpoints.MapConnectionHandler<MessagesConnectionHandler>("/chat");

    endpoints.MapGet("/deployment", async context =>
    {
        var attributes = Assembly.GetAssembly(typeof(Startup)).GetCustomAttributes<AssemblyMetadataAttribute>();

        context.Response.ContentType = "application/json";
        await using (var writer = new Utf8JsonWriter(context.Response.BodyWriter, _jsonWriterOptions))
        {
            writer.WriteStartObject();
            var commitHash = string.Empty;

            foreach (var attribute in attributes)
            {
                writer.WriteString(attribute.Key, attribute.Value);

                if (string.Equals(attribute.Key, "CommitHash"))
                {
                    commitHash = attribute.Value;
                }
            }

            if (!string.IsNullOrEmpty(commitHash))
            {
                writer.WriteString("GitHubUrl", $"https://github.com/aspnet/SignalR/commit/{commitHash}");
            }

            writer.WriteEndObject();
            await writer.FlushAsync();
        }
    });
});
*/

//app.MapHub<ChatHub>("chatHub");
/*
app.MapSignalR();

app.UseEndpoints(routes =>
{
    routes.MapHub<TestHub>("/testhub");
});
*/

/*
app.MapGet("api1", [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    (ClaimsPrincipal user) =>
{
    return user.Identity!.Name;
});
*/

/*
app.MapMethods("callback/login/github2", [HttpMethods.Get, HttpMethods.Post], async (HttpContext context) =>
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
*/

/*
app.MapMethods("authorize123", [HttpMethods.Get, HttpMethods.Post], async (HttpContext context) =>
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

app.Run();
