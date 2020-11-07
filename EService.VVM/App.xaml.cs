using EService.VVM.ViewModels;
using EService.VVM.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
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
        MainVM mainWindowViewModel;

        public App()
        {
            displayRootRegistry.RegisterWindowType<MainVM, MainWindow>();
            displayRootRegistry.RegisterWindowType<AddServiceLogVM, AddServiceLogView>();
            displayRootRegistry.RegisterWindowType<EditServiceLogVM, EditServiceLogView>();
            displayRootRegistry.RegisterWindowType<AddDeptVM, AddDeptView>();
            displayRootRegistry.RegisterWindowType<EditDeptVM, EditDeptView>();
            displayRootRegistry.RegisterWindowType<AddStatusVM, AddStatusView>();
            displayRootRegistry.RegisterWindowType<EditStatusVM, EditStatusView>();
            displayRootRegistry.RegisterWindowType<AddTypeModelVM, AddTypeModelView>();
            displayRootRegistry.RegisterWindowType<EditTypeModelVM, EditTypeModelView>();
            displayRootRegistry.RegisterWindowType<AddModelVM, AddModelView>();
            displayRootRegistry.RegisterWindowType<EditModelVM, EditModelView>();
            displayRootRegistry.RegisterWindowType<AddDeviceVM, AddDeviceView>();
            displayRootRegistry.RegisterWindowType<EditDeviceVM, EditDeviceView>();
            displayRootRegistry.RegisterWindowType<AddParameterVM, AddParameterView>();
            displayRootRegistry.RegisterWindowType<EditParameterVM, EditParameterView>();
            displayRootRegistry.RegisterWindowType<AddServiceVM, AddServiceView>();
            displayRootRegistry.RegisterWindowType<EditServiceVM, EditServiceView>();
            displayRootRegistry.RegisterWindowType<AddSpareVM, AddSpareView>();
            displayRootRegistry.RegisterWindowType<EditSpareVM, EditSpareView>();
            displayRootRegistry.RegisterWindowType<AddCategoryVM, AddCategoryView>();
            displayRootRegistry.RegisterWindowType<EditCategoryVM, EditCategoryView>();
            displayRootRegistry.RegisterWindowType<AddUserVM, AddUserView>();
            displayRootRegistry.RegisterWindowType<EditUserVM, EditUserView>();
            displayRootRegistry.RegisterWindowType<DialogVM, DialogView>();
            displayRootRegistry.RegisterWindowType<ReportVM, ReportView>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            var thread = new Thread(() =>
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke((Action)(() => new SplashScreen().Show()));
                System.Windows.Threading.Dispatcher.Run();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            mainWindowViewModel = new MainVM(new ViewModelsResolver());
            displayRootRegistry.MainVM = mainWindowViewModel;

            thread.Abort();
            base.OnStartup(e);

            await displayRootRegistry.ShowModalPresentation(mainWindowViewModel);

            Shutdown();
        }
    }
}
