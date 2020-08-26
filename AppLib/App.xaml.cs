using EService.BL;
using EService.View;
using EService.ViewModel;
using System.Windows;

namespace EService.View.WPF
{
    public partial class App : Application
    {

        public DisplayRootRegistry displayRootRegistry = new DisplayRootRegistry();
        MainViewModel mainViewModel;

        public App()
        {
            displayRootRegistry.RegisterWindowType<MainViewModel, MainWindow>();
            displayRootRegistry.RegisterWindowType<AddServiceLogViewModel, AddWindowServiceLog>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            mainViewModel = new MainViewModel();

            await displayRootRegistry.ShowModalPresentation(mainViewModel);

            Shutdown();
        }
    }
}
