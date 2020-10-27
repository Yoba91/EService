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
        //ServiceLogVM mainWindowViewModel;
        MainVM mainWindowViewModel;

        public App()
        {
            //displayRootRegistry.RegisterWindowType<ServiceLogVM, ServiceLogView>();
            displayRootRegistry.RegisterWindowType<MainVM, MainWindow>();
            displayRootRegistry.RegisterWindowType<AddServiceLogVM, AddServiceLogView>();
            displayRootRegistry.RegisterWindowType<EditServiceLogVM, EditServiceLogView>();
            displayRootRegistry.RegisterWindowType<AddDeptVM, AddDeptView>();
            displayRootRegistry.RegisterWindowType<DialogVM, DialogView>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //mainWindowViewModel = new ServiceLogVM();
            mainWindowViewModel = new MainVM(new ViewModelsResolver());

            await displayRootRegistry.ShowModalPresentation(mainWindowViewModel);

            Shutdown();
        }

    }
}
