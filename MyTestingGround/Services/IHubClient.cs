namespace MyTestingGround.Services
{
    public interface IHubClient
    {
        void DisconnectHub();
        void ConnectHub(Action<List<int>> UpdateMsg, Action<string> UpdateLog);
    }
}
