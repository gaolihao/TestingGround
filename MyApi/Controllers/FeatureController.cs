using Microsoft.AspNetCore.Mvc;
using MyApi.Data;
using Microsoft.AspNetCore.SignalR;
using MyApi.Hubs;
using MyApi.Contract;
namespace MyApi.Controllers;


[ApiController]
public class FeatureController : Controller
{
    //IHubContext<ChatHub> _hubContext;
    ISynchronizedFeatureService synchronizedScrollingService;

    public FeatureController(ISynchronizedFeatureService synchronizedScrollingService)
    {
        //_hubContext = hubcontext;
        this.synchronizedScrollingService = synchronizedScrollingService;
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
        //_hubContext.Clients.All.SendAsync("Send", Database.features);
        synchronizedScrollingService.InformSubscribers();
        return Task.FromResult<ActionResult>(Ok());
    }
}