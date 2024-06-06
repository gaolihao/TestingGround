using CommunityToolkit.Mvvm.Input;
using PropertyChanged;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyTestingGround;


[AddINotifyPropertyChangedInterface]
public partial class MainViewModel
{
    public int MyProperty { get; set; } = 1;

    [RelayCommand]
    private void Increment() => ++MyProperty;
}