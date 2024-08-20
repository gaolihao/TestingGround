using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace MyTestingGround.Services
{
    public class HubClient : IHubClient
    {
        public HubClient() 
        {
            ctsHub = new CancellationTokenSource();
        }

        private CancellationTokenSource ctsHub;

        private static async Task<bool> ConnectAsync(HubConnection connection, CancellationToken token)
        {
            // Keep trying to until we can start
            while (true)
            {
                try
                {
                    await connection.StartAsync(token);
                    return true;
                }
                catch when (token.IsCancellationRequested)
                {
                    return false;
                }
                catch
                {
                    Console.WriteLine("Failed to connect, trying again in 5000(ms)");

                    await Task.Delay(5000);
                }
            }
        }

        public void DisconnectHub() 
        {
            ctsHub.Cancel();
            ctsHub = new CancellationTokenSource();
        }

        public void ConnectHub(Action<List<int>> UpdateMsg, Action<string> UpdateLog) 
        {
            var ct = ctsHub.Token;
            Task.Run(async () =>
            {
                var uri = "http://localhost:5232/default";

                UpdateLog("Connecting to " + uri);

                var connectionBuilder = new HubConnectionBuilder();

                connectionBuilder.Services.Configure<LoggerFilterOptions>(options =>
                {
                    options.MinLevel = LogLevel.Trace;
                });

                connectionBuilder.WithUrl(uri);

                connectionBuilder.WithAutomaticReconnect();

                using var closedTokenSource = new CancellationTokenSource();
                var connection = connectionBuilder.Build();
                
                try
                {
                    Console.CancelKeyPress += (sender, a) =>
                    {
                        a.Cancel = true;
                        closedTokenSource.Cancel();
                        connection.StopAsync().GetAwaiter().GetResult();
                    };

                    // Set up handler
                    connection.On<List<int>>("Send", UpdateMsg);

                    connection.Closed += e =>
                    {
                        Console.WriteLine("Connection closed...");
                        closedTokenSource.Cancel();
                        return Task.CompletedTask;
                    };

                    if (!await ConnectAsync(connection, closedTokenSource.Token))
                    {
                        Console.WriteLine("Failed to establish a connection to '{0}' because the CancelKeyPress event fired first. Exiting...", uri);
                        return ;
                    }

                    UpdateLog("Connected to " + uri);

                    while (true)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            UpdateLog("Ending Connection");
                            break;
                        }
                    }
                    /*
                    // Handle the connected connection
                    while (true)
                    {
                        // If the underlying connection closes while waiting for user input, the user will not observe
                        // the connection close aside from "Connection closed..." being printed to the console. That's
                        // because cancelling Console.ReadLine() is a royal pain.
                        var line = Console.ReadLine();

                        if (line == null || closedTokenSource.Token.IsCancellationRequested)
                        {
                            Console.WriteLine("Exiting...");
                            break;
                        }

                        try
                        {
                            await connection.InvokeAsync<object>("Send", "C#", line);
                        }
                        catch when (closedTokenSource.IsCancellationRequested)
                        {
                            // We're shutting down the client
                            Console.WriteLine("Failed to send '{0}' because the CancelKeyPress event fired first. Exiting...", line);
                            break;
                        }
                        catch (Exception ex)
                        {
                            // Send could have failed because the connection closed
                            // Continue to loop because we should be reconnecting.
                            Console.WriteLine(ex);
                        }
                    }
                    */
                }
                finally
                {
                    await connection.StopAsync();
                }
            }, ct);
        }
    }
}
