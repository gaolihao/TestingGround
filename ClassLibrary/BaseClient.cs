namespace ClassLibrary;

public class BaseClient
{

    public BaseClient()
    {
    }

    public Task<HttpRequestMessage> CreateHttpRequestMessageAsync(CancellationToken _)
    {
        var request = new HttpRequestMessage()
        {
            Version = new Version(2, 0)
        };
        
        return Task.FromResult(request);
    }
}
