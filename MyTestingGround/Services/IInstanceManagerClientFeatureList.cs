using MyApi.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTestingGround.Services;
public interface IInstanceManagerClientFeatureList
{
    Task<IDictionary<int, AttachableProcess>> GetProcessesAsync();
    void Connect(Action<FeatureList> locationUpdatedByServer);
    void ReportLocation(FeatureList location);
    Task SubscribeAsync(int processId);
    void Disconnect();
}
