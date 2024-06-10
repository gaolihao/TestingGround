using System.Security.Claims;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Dapplo.Microsoft.Extensions.Hosting.Wpf;
using OpenIddict.Client;
using System.Net.Http.Headers;
using System.Net.Http;
using MyNamespace;

namespace MyTestingGround;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, IWpfShell
{
    private readonly OpenIddictClientService _service;

    public MainWindow(OpenIddictClientService service)
    {
        DataContext = new MainViewModel();
        _service = service ?? throw new ArgumentNullException(nameof(service));

        InitializeComponent();
    }

    static async Task<string> GetResourceAsync(string token, CancellationToken cancellationToken = default)
    {
        var httpClient = new HttpClient();
        var client = new MyNamespace.Client("http://localhost:5232/", httpClient);
        var result = await client.UsernameAsync(cancellationToken);

        return result.Value;
        /*
        using var client = new HttpClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:5232/api");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
        */
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        // Disable the login button to prevent concurrent authentication operations.
        LoginButton.IsEnabled = false;

        try
        {
            using var source = new CancellationTokenSource(delay: TimeSpan.FromSeconds(90));

            try
            {
                // Ask OpenIddict to initiate the authentication flow (typically, by starting the system browser).
                var result = await _service.ChallengeInteractivelyAsync(new()
                {
                    CancellationToken = source.Token
                });

                // Wait for the user to complete the authorization process.
                var response = (await _service.AuthenticateInteractivelyAsync(new()
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
            LoginButton.IsEnabled = true;
        }
    }
}