using CommunityToolkit.Mvvm.Input;
using PropertyChanged;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;

namespace MyTestingGround;


[AddINotifyPropertyChangedInterface]
public partial class MainViewModel
{
    public int MyProperty { get; set; } = 1;

    [RelayCommand]
    private void Increment() => ++MyProperty;

    public string s { get; set; } = "";

    [RelayCommand]
    private async Task ExtractAsync()
    {
        var httpClient = new HttpClient();
        var client = new MyNamespace.Client("http://localhost:5232/", httpClient);
        var results = await client.TodoitemsAllAsync();
        s = string.Join("\n", results.Select(todo => todo.Name));
    }
}