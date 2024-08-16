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
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.SignalR.Client;

namespace MyTestingGround;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly OpenIddictClientService _service;
    private HubConnection connection;

    MainViewModel ViewModel => DataContext as MainViewModel ?? throw new InvalidOperationException("Can't cast");

    /*
    public MainWindow(OpenIddictClientService service)
    {
        DataContext = new MainViewModel();
        _service = service ?? throw new ArgumentNullException(nameof(service));

        InitializeComponent();
    }
    */

    public MainWindow()
    {
        InitializeComponent();

        connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5232/TestHub")
                .Build();
        connection.Closed += async (error) =>
        {
            await Task.Delay(new Random().Next(0, 5) * 1000);
            await connection.StartAsync();
        };
    }

    private async void BtnConnect_Click(object sender, RoutedEventArgs e)
    {

        /*
        connection.On<string>("Connected",
                               (connectionid) =>
                               {
                                   //MessageBox.Show(connectionid);
                                   tbMain.Text = connectionid;
                               });

        connection.On<string>("Posted",
                               (value) =>
                               {
                                   Dispatcher.BeginInvoke((Action)(() =>

                                   {
                                       messagesList.Items.Add(value);
                                   }));
                               });
        try
        {
            await connection.StartAsync();
            messagesList.Items.Add("Connection started");
            btnConnect.IsEnabled = false;
            //sendButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            messagesList.Items.Add(ex.Message);
        }
        */
    }

    private async void connectButton_Click(object sender, RoutedEventArgs e)
    {
        connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            this.Dispatcher.Invoke(() =>                                                                                                                                                                                                                                                                                                                      
            {
                var newMessage = $"{user}: {message}";
                messagesList.Items.Add(newMessage);
            });
        });

        try
        {
            await connection.StartAsync();
            messagesList.Items.Add("Connection started");
            //connectButton.IsEnabled = false;
            //sendButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            messagesList.Items.Add(ex.Message);
        }
    }

    private async void sendButton_Click(object sender, RoutedEventArgs e)
    {
        /*
        try
        {
            await connection.InvokeAsync("SendMessage",
                userTextBox.Text, messageTextBox.Text);
        }
        catch (Exception ex)
        {
            messagesList.Items.Add(ex.Message);
        }
        */
    }
}