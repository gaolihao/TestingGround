/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:NoviView.Wpf"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

namespace MyTestingGround
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {

#pragma warning disable CA1822 // Mark members as static
        public IMainViewModel Main => MainStatic;
        public static IMainViewModel MainStatic => GetViewModelInstance<IMainViewModel>();

        public static T GetViewModelInstance<T>() => App.GetViewModelInstance<T>();
    }
}
