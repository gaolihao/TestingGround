using MyApi.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTestingGround.Services
{
    public interface IInstanceManagerClient
    {
        Task<IDictionary<int, AttachableProcess>> GetProcessesAsync();
        void Connect(string filename, Action<Location> locationUpdatedByServer);
        void ReportLocation(Location location);
        Task SubscribeAsync(int processId);
        void Disconnect();
    }
}
