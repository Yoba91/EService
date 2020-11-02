using EService.VVM.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EService.VVM.ViewModels
{
    public class MainVM : INotifyPropertyChanged
    {
        #region Константы
        public static readonly string ServiceLogVMAlias = "ServiceLogVM";
        public static readonly string TypeModelVMAlias = "TypeModelVM";
        public static readonly string ModelVMAlias = "ModelVM";
        public static readonly string DeviceVMAlias = "DeviceVM";
        public static readonly string ParameterVMAlias = "ParameterVM";
        public static readonly string SpareVMAlias = "SpareVM";
        public static readonly string ServiceVMAlias = "ServiceVM";
        public static readonly string DeptVMAlias = "DeptVM";
        public static readonly string StatusVMAlias = "StatusVM";
        public static readonly string CategoryVMAlias = "CategoryVM";
        public static readonly string UserVMAlias = "UserVM";
        public static readonly string NotFoundPageVMAlias = "Page404VM";
        #endregion

        #region Поля
        private readonly IViewModelsResolver _resolver;

        private IList<BaseVM> VMs;

        private ICommand _goToPathCommand;
        private ICommand _goToServiceLogCommand;
        private ICommand _goToTypeModelCommand;
        private ICommand _goToModelCommand;
        private ICommand _goToDeviceCommand;
        private ICommand _goToParameterCommand;
        private ICommand _goToSpareCommand;
        private ICommand _goToServiceCommand;
        private ICommand _goToDeptCommand;
        private ICommand _goToStatusCommand;
        private ICommand _goToCategoryCommand;
        private ICommand _goToUserCommand;

        private readonly INotifyPropertyChanged _serviceLogVM;
        private readonly INotifyPropertyChanged _typeModelVM;
        private readonly INotifyPropertyChanged _modelVM;
        private readonly INotifyPropertyChanged _deviceVM;
        private readonly INotifyPropertyChanged _parameterVM;
        private readonly INotifyPropertyChanged _spareVM;
        private readonly INotifyPropertyChanged _serviceVM;
        private readonly INotifyPropertyChanged _deptVM;
        private readonly INotifyPropertyChanged _statusVM;
        private readonly INotifyPropertyChanged _categoryVM;
        private readonly INotifyPropertyChanged _userVM;
        #endregion

        #region Свойства

        public ICommand GoToPathCommand
        {
            get { return _goToPathCommand; }
            set
            {
                _goToPathCommand = value;
                RaisePropertyChanged("GoToPathCommand");
            }
        }

        public ICommand GoToServiceLogCommand
        {
            get
            {
                return _goToServiceLogCommand;
            }
            set
            {
                _goToServiceLogCommand = value;
                RaisePropertyChanged("GoToServiceLogCommand");
            }
        }

        public ICommand GoToTypeModelCommand
        {
            get { return _goToTypeModelCommand; }
            set
            {
                _goToTypeModelCommand = value;
                RaisePropertyChanged("GoToTypeModelCommand");
            }
        }

        public ICommand GoToModelCommand
        {
            get { return _goToModelCommand; }
            set
            {
                _goToModelCommand = value;
                RaisePropertyChanged("GoToModelCommand");
            }
        }

        public ICommand GoToDeviceCommand
        {
            get { return _goToDeviceCommand; }
            set
            {
                _goToDeviceCommand = value;
                RaisePropertyChanged("GoToDeviceCommand");
            }
        }

        public ICommand GoToParameterCommand
        {
            get { return _goToParameterCommand; }
            set
            {
                _goToParameterCommand = value;
                RaisePropertyChanged("GoToParameterCommand");
            }
        }

        public ICommand GoToSpareCommand
        {
            get { return _goToSpareCommand; }
            set
            {
                _goToSpareCommand = value;
                RaisePropertyChanged("GoToSpareCommand");
            }
        }

        public ICommand GoToServiceCommand
        {
            get { return _goToServiceCommand; }
            set
            {
                _goToServiceCommand = value;
                RaisePropertyChanged("GoToServiceCommand");
            }
        }

        public ICommand GoToDeptCommand
        {
            get { return _goToDeptCommand; }
            set
            {
                _goToDeptCommand = value;
                RaisePropertyChanged("GoToDeptCommand");
            }
        }

        public ICommand GoToStatusCommand
        {
            get { return _goToStatusCommand; }
            set
            {
                _goToStatusCommand = value;
                RaisePropertyChanged("GoToStatusCommand");
            }
        }

        public ICommand GoToCategoryCommand
        {
            get { return _goToCategoryCommand; }
            set
            {
                _goToCategoryCommand = value;
                RaisePropertyChanged("GoToCategoryCommand");
            }
        }

        public ICommand GoToUserCommand
        {
            get { return _goToUserCommand; }
            set
            {
                _goToUserCommand = value;
                RaisePropertyChanged("GoToUserCommand");
            }
        }

        public INotifyPropertyChanged ServiceLogVM
        {
            get { return _serviceLogVM; }
        }
        public INotifyPropertyChanged TypeModelVM
        {
            get { return _typeModelVM; }
        }
        public INotifyPropertyChanged ModelVM
        {
            get { return _modelVM; }
        }
        public INotifyPropertyChanged DeviceVM
        {
            get { return _deviceVM; }
        }
        public INotifyPropertyChanged ParameterVM
        {
            get { return _parameterVM; }
        }
        public INotifyPropertyChanged SpareVM
        {
            get { return _spareVM; }
        }
        public INotifyPropertyChanged ServiceVM
        {
            get { return _serviceVM; }
        }
        public INotifyPropertyChanged DeptVM
        {
            get { return _deptVM; }
        }
        public INotifyPropertyChanged StatusVM
        {
            get { return _statusVM; }
        }
        public INotifyPropertyChanged CategoryVM
        {
            get { return _categoryVM; }
        }
        public INotifyPropertyChanged UserVM
        {
            get { return _userVM; }
        }

        #endregion

        #region Конструктор
        public MainVM(IViewModelsResolver resolver)
        {
            string executable = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string path = (System.IO.Path.GetDirectoryName(executable));
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            _resolver = resolver;

            _serviceLogVM = _resolver.GetViewModelInstance(ServiceLogVMAlias);
            _typeModelVM = _resolver.GetViewModelInstance(TypeModelVMAlias);
            _modelVM = _resolver.GetViewModelInstance(ModelVMAlias);
            _deviceVM = _resolver.GetViewModelInstance(DeviceVMAlias);
            _parameterVM = _resolver.GetViewModelInstance(ParameterVMAlias);
            _spareVM = _resolver.GetViewModelInstance(SpareVMAlias);
            _serviceVM = _resolver.GetViewModelInstance(ServiceVMAlias);
            _deptVM = _resolver.GetViewModelInstance(DeptVMAlias);
            _statusVM = _resolver.GetViewModelInstance(StatusVMAlias);
            _categoryVM = _resolver.GetViewModelInstance(CategoryVMAlias);
            _userVM = _resolver.GetViewModelInstance(UserVMAlias);

            VMs = new List<BaseVM>();

            VMs.Add((BaseVM)_serviceLogVM);
            VMs.Add((BaseVM)_typeModelVM);
            VMs.Add((BaseVM)_modelVM);
            VMs.Add((BaseVM)_deviceVM);
            VMs.Add((BaseVM)_parameterVM);
            VMs.Add((BaseVM)_spareVM);
            VMs.Add((BaseVM)_serviceVM);
            VMs.Add((BaseVM)_deptVM);
            VMs.Add((BaseVM)_statusVM);
            VMs.Add((BaseVM)_categoryVM);
            VMs.Add((BaseVM)_userVM);

            InitializeCommands();

            GoToServiceLogCommand.Execute(ServiceLogVM);
        }
        #endregion
        public void Refresh()
        {
            foreach (var item in VMs)
            {
                item.Refresh();
            }
        }
        private void InitializeCommands()
        {

            GoToPathCommand = new RelayCommand<string>(GoToPathCommandExecute);

            GoToServiceLogCommand = new RelayCommand<INotifyPropertyChanged>(GoToServiceLogCommandExecute);
            GoToTypeModelCommand = new RelayCommand<INotifyPropertyChanged>(GoToTypeModelCommandExecute);
            GoToModelCommand = new RelayCommand<INotifyPropertyChanged>(GoToModelCommandExecute);
            GoToDeviceCommand = new RelayCommand<INotifyPropertyChanged>(GoToDeviceCommandExecute);
            GoToParameterCommand = new RelayCommand<INotifyPropertyChanged>(GoToParameterCommandExecute);
            GoToSpareCommand = new RelayCommand<INotifyPropertyChanged>(GoToSpareCommandExecute);
            GoToServiceCommand = new RelayCommand<INotifyPropertyChanged>(GoToServiceCommandExecute);
            GoToDeptCommand = new RelayCommand<INotifyPropertyChanged>(GoToDeptCommandExecute);
            GoToStatusCommand = new RelayCommand<INotifyPropertyChanged>(GoToStatusCommandExecute);
            GoToCategoryCommand = new RelayCommand<INotifyPropertyChanged>(GoToCategoryCommandExecute);
            GoToUserCommand = new RelayCommand<INotifyPropertyChanged>(GoToUserCommandExecute);
        }

        private void GoToPathCommandExecute(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                return;
            }

            Navigation.Navigation.Navigate(alias);
        }

        private void GoToServiceLogCommandExecute(INotifyPropertyChanged viewModel)
        {
            Navigation.Navigation.Navigate(Navigation.Navigation.ServiceLogAlias, ServiceLogVM);
        }

        private void GoToTypeModelCommandExecute(INotifyPropertyChanged viewModel)
        {
            Navigation.Navigation.Navigate(Navigation.Navigation.TypeModelAlias, TypeModelVM);
        }

        private void GoToModelCommandExecute(INotifyPropertyChanged viewModel)
        {
            Navigation.Navigation.Navigate(Navigation.Navigation.ModelAlias, ModelVM);
        }

        private void GoToDeviceCommandExecute(INotifyPropertyChanged viewModel)
        {
            Navigation.Navigation.Navigate(Navigation.Navigation.DeviceAlias, DeviceVM);
        }

        private void GoToParameterCommandExecute(INotifyPropertyChanged viewModel)
        {
            Navigation.Navigation.Navigate(Navigation.Navigation.ParameterAlias, ParameterVM);
        }

        private void GoToSpareCommandExecute(INotifyPropertyChanged viewModel)
        {
            Navigation.Navigation.Navigate(Navigation.Navigation.SpareAlias, SpareVM);
        }

        private void GoToServiceCommandExecute(INotifyPropertyChanged viewModel)
        {
            Navigation.Navigation.Navigate(Navigation.Navigation.ServiceAlias, ServiceVM);
        }

        private void GoToDeptCommandExecute(INotifyPropertyChanged viewModel)
        {
            Navigation.Navigation.Navigate(Navigation.Navigation.DeptAlias, DeptVM);
        }

        private void GoToStatusCommandExecute(INotifyPropertyChanged viewModel)
        {
            Navigation.Navigation.Navigate(Navigation.Navigation.StatusAlias, StatusVM);
        }

        private void GoToCategoryCommandExecute(INotifyPropertyChanged viewModel)
        {
            Navigation.Navigation.Navigate(Navigation.Navigation.CategoryAlias, CategoryVM);
        }

        private void GoToUserCommandExecute(INotifyPropertyChanged viewModel)
        {
            Navigation.Navigation.Navigate(Navigation.Navigation.UserAlias, UserVM);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return;
            }

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
