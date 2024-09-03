using CommunityToolkit.Mvvm.Input;
using PropertyChanged;
using System.Net.Http;
using System.Security.Claims;
using System.Windows;
using OpenIddict.Client;
using System.Net.Http.Headers;
using MyTestingGround.Services;
using Microsoft.Extensions.Logging;
using MyApi.Contract;
using Grpc.Net.Client;
using MyApi.Services;
using ClassLibrary;

namespace MyTestingGround;


[AddINotifyPropertyChangedInterface]
public partial class MainViewModel : IMainViewModel
{

    IHubClient hubClient;

    IInstanceManagerClient instanceManagerClient;

    
    public MainViewModel(IInstanceManagerClient instanceManagerClientFeatureList)
    {
        //this.hubClient = hubClient;
        instanceManagerClient = instanceManagerClientFeatureList;

    }
    

    /*
    public MainViewModel(IInstanceManagerClientFeatureList instanceManagerClient, IHubClient hubClient)
    {
        this.instanceManagerClient = instanceManagerClient;
        this.hubClient = hubClient;
    }
    */
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
        var client = new Client("http://localhost:5232/", httpClient);
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
        var client = new Client("http://localhost:5232/", httpClient);
        var result = await client.UsernameAsync(cancellationToken);

        return result.ToString();
    }

    public IEnumerable<int> features { get; set; } = [];

    public string featuresS => "Feature Data: " + string.Join(",", features.ToArray());

    [RelayCommand]
    private async Task GetFeaturesAsync()
    {
        var httpClient = new HttpClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:7204/features")
        {
            Version = new Version(2, 0)
        };
        

        using var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var a = await response.Content.ReadAsStringAsync();
        msg = a.ToString();
        
        //var client = new MyNamespace.Client("https://localhost:7204/", httpClient);
        //features = await client.FeaturesAsync();
    }

    [RelayCommand]
    private async Task AddFeatureAsync()
    {
        var httpClient = new HttpClient()
        {
            DefaultRequestVersion = new Version(2, 0)
        };
        var client = new Client("https://localhost:7204/", httpClient);
        await client.FeatureAsync(MyProperty);

        // Get features
        features = await client.FeaturesAsync();

        instanceManagerClient.ReportLocation(new(MyProperty));
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

        /*
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        var httpClient = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
        var channel = GrpcChannel.ForAddress("https://localhost:7204",
            new GrpcChannelOptions { HttpClient = httpClient });
        var client = new InstanceManagerClientFeatureList(channel);
        */
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

    private async void FeatureListUpdated(FeatureList featureList)
    {
        features.Append((int)featureList.WeldIdFine);
    }

    [RelayCommand]
    private async Task ConnectGRPCAsync()
    {
        instanceManagerClient.Connect("", FeatureListUpdated);
    }

    [RelayCommand]
    private async Task DisconnectGRPCAsync()
    {
        instanceManagerClient.Disconnect();
    }


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