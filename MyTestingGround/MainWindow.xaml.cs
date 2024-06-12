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

namespace MyTestingGround;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly OpenIddictClientService _service;

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
    }
}