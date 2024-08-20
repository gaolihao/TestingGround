using Microsoft.AspNetCore.Mvc;
using MyApi.Data;
using Microsoft.AspNetCore.SignalR;
using MyApi.Hubs;
namespace MyApi.Controllers;


[ApiController]
public class FeatureController : Controller
{
    IHubContext<ChatHub> _hubContext;

    public FeatureController(IHubContext<ChatHub> hubcontext)
    {
        _hubContext = hubcontext;
    }

    [HttpGet("features")]
    public List<int> Get()
    {
        return Database.features;

    }

    [HttpPost("feature")]
    public Task<ActionResult> Post([FromBody] int feature)
    {
        Database.features.Add(feature);
        _hubContext.Clients.All.SendAsync("Send", Database.features);
        return Task.FromResult<ActionResult>(Ok());
    }
}