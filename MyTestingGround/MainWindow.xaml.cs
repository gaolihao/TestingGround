using System.Windows;
using OpenIddict.Client;
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

    public MainWindow()
    {
        InitializeComponent();
    }
}