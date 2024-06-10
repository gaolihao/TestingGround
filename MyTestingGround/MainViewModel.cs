using CommunityToolkit.Mvvm.Input;
using MyNamespace;
using PropertyChanged;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Claims;
using System.Windows;

namespace MyTestingGround;


[AddINotifyPropertyChangedInterface]
public partial class MainViewModel
{
    Client client;
    public MainViewModel()
    {
        var httpClient = new HttpClient();
        client = new MyNamespace.Client("http://localhost:5232/", httpClient);
    }
    public int MyProperty { get; set; } = 1;

    [RelayCommand]
    private void Increment() => ++MyProperty;

    public string s { get; set; } = "";

    [RelayCommand]
    private async Task ExtractAsync()
    {
        
        var results = await client.TodoAllAsync();
        s = string.Join("\n", results.Select(todo => todo.Name));
    }
    public string username { get; set; } = "";

    [RelayCommand]
    private async Task GetUsernameAsync()
    {
        var results = await client.UsernameAsync();
        username = results.Value;
    }


    [RelayCommand]
    private async Task AuthorizeAsync()
    {

        //var results = await client.LoginPOSTAsync(provider);
        //s = string.Join("\n", results.Select(todo => todo.Name));
    }

}