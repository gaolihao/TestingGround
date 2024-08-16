using Microsoft.AspNetCore.Mvc;
using MyApi.Models;
using MyApi.Data;
using SignalRSamples.Hubs;
using Microsoft.AspNetCore.SignalR;
using MyApi.Hubs;
namespace MyApi.Server;


[ApiController]
public class FeatureController : Controller
{
    IHubContext<Chat> _hubContext;

    public FeatureController(IHubContext<Chat> hubcontext)
    {
        _hubContext = hubcontext;
    }

    [HttpGet("features")]
    //[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public List<int> Get()
    {
        return Database.features;

    }

    [HttpPost("feature")]
    public Task<ActionResult> Post([FromBody] int feature)
    {
        Database.features.Add(feature);
        _hubContext.send
        return Task.FromResult<ActionResult>(Ok());
    }

}