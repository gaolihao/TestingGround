using CommunityToolkit.Mvvm.Input;
using MyNamespace;
using PropertyChanged;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Claims;
using System.Windows;
using OpenIddict.Client;
using System.Net.Http.Headers;
using System.Threading;

namespace MyTestingGround;


[AddINotifyPropertyChangedInterface]
public partial class MainViewModel : IMainViewModel
{
    Client client;
    public MainViewModel(OpenIddictClientService service)
    {
        var httpClient = new HttpClient();
        client = new MyNamespace.Client("http://localhost:5232/", httpClient);
        Service = service;
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