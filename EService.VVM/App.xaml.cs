using EService.VVM.ViewModels;
using EService.VVM.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EService.VVM
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {

        public DisplayRootRegistry displayRootRegistry = new DisplayRootRegistry();
        ServiceLogVM mainWindowViewModel;

        public App()
        {
            displayRootRegistry.RegisterWindowType<ServiceLogVM, ServiceLogView>();
            displayRootRegistry.RegisterWindowType<AddServiceLogVM, AddServiceLogView>();
            displayRootRegistry.RegisterWindowType<DialogVM, DialogView>();
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
