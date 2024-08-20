using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace MyTestingGround.Services
{
    public interface IHubClient
    {
        void DisconnectHub();
        void ConnectHub(Action<List<int>> UpdateMsg, Action<string> UpdateLog);
    }
}
