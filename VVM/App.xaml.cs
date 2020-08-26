using System.Windows;
using VVM.Views;
using VVM.ViewModels;
using System.Threading.Tasks;
using VVM;

namespace MVVM_OpenNewWindowMinimalExample {
    public partial class App : Application {

        public DisplayRootRegistry displayRootRegistry = new DisplayRootRegistry();
        ServiceLogVM mainWindowViewModel;

        public App()
        {
            displayRootRegistry.RegisterWindowType<ServiceLogVM, ServiceLogView>();            
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            mainWindowViewModel = new ServiceLogVM();

            await displayRootRegistry.ShowModalPresentation(mainWindowViewModel);

            Shutdown();
        }
    }
}
