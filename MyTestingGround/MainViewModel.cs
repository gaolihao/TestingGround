using CommunityToolkit.Mvvm.Input;
using PropertyChanged;
using System.Net.Http;
using System.Security.Claims;
using System.Windows;
using OpenIddict.Client;
using System.Net.Http.Headers;
using System.IO.Pipelines;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MyTestingGround.Services;

namespace MyTestingGround;


[AddINotifyPropertyChangedInterface]
public partial class MainViewModel : IMainViewModel
{

    IHubClient hubClient;

    public MainViewModel(IHubClient hubClient)
    {
        this.hubClient = hubClient;
        //var httpClient = new HttpClient();
        //client = new MyNamespace.Client("http://localhost:5232/", httpClient);
        //Service = service;
    }
    public int MyProperty { get; set; } = 1;

    [RelayCommand]
    private void Increment() => ++MyProperty;

    public string s { get; set; } = "";

    [RelayCommand]
    private async Task ExtractAsync()
    {

        //var results = await client.TodoAllAsync();
        //s = string.Join("\n", results.Select(todo => todo.Name));
    }
    public string username { get; set; } = "";
    public OpenIddictClientService Service { get; }

    [RelayCommand]
    private async Task GetUsernameAsync()
    {
        //var results = await GetResourceAsync();
        //username = results.Value;
        var httpClient = new HttpClient();
        var client = new MyNamespace.Client("http://localhost:5232/", httpClient);
        var result = await client.UsernameAsync();
        username = result.ToString();
    }


    [RelayCommand]
    private async Task AuthorizeAsync()
    {

        //var results = await client.LoginPOSTAsync(provider);
        //s = string.Join("\n", results.Select(todo => todo.Name));
    }

    private async Task<string> GetResourceAsync(string token, CancellationToken cancellationToken = default)
    {

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var client = new MyNamespace.Client("http://localhost:5232/", httpClient);
        var result = await client.UsernameAsync(cancellationToken);

        return result.ToString();


        /*
        using var client = new HttpClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:5232/username");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
        */
    }

    public IEnumerable<int> features { get; set; } = [];

    public string featuresS => "Feature Data: " + string.Join(",", features.ToArray());

    [RelayCommand]
    private async Task GetFeaturesAsync()
    {
        var httpClient = new HttpClient();
        var client = new MyNamespace.Client("http://localhost:5232/", httpClient);
        features = await client.FeaturesAsync();
    }

    [RelayCommand]
    private async Task AddFeatureAsync()
    {
        var httpClient = new HttpClient();
        var client = new MyNamespace.Client("http://localhost:5232/", httpClient);
        await client.FeatureAsync(1);
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        hubClient.ConnectHub(ValidateInput, UpdateLog);
    }

    [RelayCommand]
    private async Task DisconnectAsync()
    {
        hubClient.DisconnectHub();
    }

    public string msg { get; set; } = "";
    public void ValidateInput(List<int> input)
    {
        features = input;
    }

    public void UpdateLog(string message)
    {
        msg += message + "\n";
    }

    /*
    private async Task<int> ExecuteAsync()
    {
        var uri = "http://localhost:5232/default";

        msg += ("Connecting to {0}", uri);

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
            connection.On<string>("Send", ValidateInput);

            
            connection.Closed += e =>
            {
                Console.WriteLine("Connection closed...");
                closedTokenSource.Cancel();
                return Task.CompletedTask;
            };
            

            if (!await ConnectAsync(connection, closedTokenSource.Token))
            {
                //Console.WriteLine("Failed to establish a connection to '{0}' because the CancelKeyPress event fired first. Exiting...", uri);
                return 0;
            }

            msg += ("Connected to {0}", uri);

            while (true)
            {

            }

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
            
            
        }
        finally
        {
            await connection.StopAsync();
        }

        return 0;
    }

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
*/


    /*
    private async Task ConnectsAsync()
    {
        var baseUrl = "http://localhost:5232/default";
        Console.WriteLine($"Connecting to {baseUrl}...");

        var connectionOptions = new HttpConnectionOptions
        {
            Url = new Uri(baseUrl),
            DefaultTransferFormat = TransferFormat.Text,
        };

        var connection = new HttpConnection(connectionOptions, loggerFactory: null);

        try
        {
            await connection.StartAsync();

            //Console.WriteLine($"Connected to {baseUrl}");
            var shutdown = new TaskCompletionSource<object>();
            

            _ = ReceiveLoop(connection.Transport.Input);
            //_ = SendLoop(Console.In, connection.Transport.Output);

            await shutdown.Task;
        }
        catch (AggregateException aex) when (aex.InnerExceptions.All(e => e is OperationCanceledException))
        {
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            await connection.DisposeAsync();
        }

    }
    */

    /*
    private async Task ReceiveLoop(PipeReader input)
    {
        while (true)
        {
            var result = await input.ReadAsync();
            var buffer = result.Buffer;

            try
            {
                if (!buffer.IsEmpty)
                {
                    var s = Encoding.UTF8.GetString(buffer);
                    msg = s;
                }
                else if (result.IsCompleted)
                {
                    // No more data, and the pipe is complete
                    break;
                }
            }
            finally
            {
                input.AdvanceTo(buffer.End);
            }
        }
    }

    private static async Task SendLoop(TextReader input, PipeWriter output)
    {
        while (true)
        {
            var result = await input.ReadLineAsync();
            await output.WriteAsync(Encoding.UTF8.GetBytes(result));
        }
    }
    */


    [RelayCommand]
    private async Task LoginAsync()
    {
        // Disable the login button to prevent concurrent authentication operations.
        try
        {
            using var source = new CancellationTokenSource(delay: TimeSpan.FromSeconds(90));

            try
            {
                // Ask OpenIddict to initiate the authentication flow (typically, by starting the system browser).
                var result = await Service.ChallengeInteractivelyAsync(new()
                {
                    CancellationToken = source.Token
                });

                // Wait for the user to complete the authorization process.
                var response = (await Service.AuthenticateInteractivelyAsync(new()
                {
                    CancellationToken = source.Token,
                    Nonce = result.Nonce
                }));
                var principal = response.Principal;

                var token = await GetResourceAsync(response.BackchannelAccessToken ?? response.FrontchannelAccessToken, source.Token);
                MessageBox.Show($"Welcome, {principal.FindFirst(ClaimTypes.Name)!.Value}. Your GitHub identifier is: {token}",
                    "Authentication successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            catch (OperationCanceledException)
            {
                MessageBox.Show("The authentication process was aborted.",
                "Authentication timed out", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            catch
            {
                MessageBox.Show("An error occurred while trying to authenticate the user.",
                    "Authentication failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        finally
        {
            // Re-enable the login button to allow starting a new authentication operation.
            // LoginButton.IsEnabled = true;
        }
    }

}