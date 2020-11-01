using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EService.Data.Entity;
using EService.BL;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;

namespace EService.VVM.ViewModels
{
    public class AddServiceLogVM : BaseVM
    {
        #region Поля
        private DbContext _dbContext;
        private string _search;
        private FilterSearch _filterSearch;
        ParameterExpression _parameter;
        private IList<IFilter> _filters;

        private DateTime _date;

        private IList<Device> _devices;

        private Device _selectedDevice;
        private ParameterValue _selectedParameterValue;
        private SpareForModel _selectedSpare;
        private ServiceForModel _selectedService;
        private Repairer _selectedUser;

        private IList<ServiceForModel> _selectedServices;
        private IList<SpareForModel> _selectedSpares;

        private IDelegateCommand _addServiceLog;
        #endregion
        #region Свойства
        public ObservableCollection<ServiceForModel> SelectedServices
        {
            get { return (ObservableCollection<ServiceForModel>)_selectedServices; }
            set
            {
                _selectedServices = value;
                this.AddServiceLog.RaiseCanExecuteChanged();
            }
        }
        public ObservableCollection<SpareForModel> SelectedSpares
        {
            get { return (ObservableCollection<SpareForModel>)_selectedSpares; }
            set
            {
                _selectedSpares = value;
            }
        }
        public IList<Device> Devices { get { return _devices; } set { _devices = value; OnPropertyChanged("Devices"); } }
        public ObservableCollection<ParameterValue> ParametersValues { get; set; }
        public ObservableCollection<ServiceForModel> Services { get; set; }
        public ObservableCollection<SpareForModel> Spares { get; set; }
        public IList<Repairer> Users { get; set; }
        public Device SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                _selectedDevice = value;
                if (_selectedDevice != null)
                {
                    ParametersValues.Clear();
                    Services.Clear();
                    Spares.Clear();
                    if (_dbContext is SQLiteContext)
                    {
                        SQLiteContext context = _dbContext as SQLiteContext;
                        var parameters = context.ParameterForModel.Where(pfm => pfm.RowidModel == _selectedDevice.RowidModel).ToList();
                        foreach (var item in parameters)
                        {
                            var sl = context.ServiceLog.Where(s => s.Device.Rowid == SelectedDevice.Rowid).ToList().LastOrDefault();
                            if(sl != null)
                            {
                                var pv = context.ParameterValue.Where(p => p.RowidServiceLog == sl.Rowid).ToList().LastOrDefault();
                                if (pv != null)
                                {
                                    ParametersValues.Add(new ParameterValue() { RowidParameterForModel = item.Rowid, ParameterForModel = item, Value = pv.Value });                                    
                                }
                                else
                                {
                                    ParametersValues.Add(new ParameterValue() { RowidParameterForModel = item.Rowid, ParameterForModel = item, Value = item.Parameter.Default });
                                }
                            }
                            else
                            {
                                ParametersValues.Add(new ParameterValue() { RowidParameterForModel = item.Rowid, ParameterForModel = item, Value = item.Parameter.Default });
                            }
                        }
                        var tempServices = context.ServiceForModel.Where(s => s.RowidModel == _selectedDevice.RowidModel).ToList();
                        foreach (var item in tempServices)
                        {
                            Services.Add(item);
                        }
                        var tempSpares = context.SpareForModel.Where(s => s.RowidModel == _selectedDevice.RowidModel).ToList();
                        foreach (var item in tempSpares)
                        {
                            Spares.Add(item);
                        }
                    }
                }
            }
        }
        public ParameterValue SelectedParameterValue { get { return _selectedParameterValue; } set { _selectedParameterValue = value; } }
        public SpareForModel SelectedSpare { get { return _selectedSpare; } set { _selectedSpare = value; } }
        public ServiceForModel SelectedService { get { return _selectedService; } set { _selectedService = value; } }
        public Repairer SelectedUser { get { return _selectedUser; } set { _selectedUser = value; } }
        public String Search
        {
            get { return _search; }
            set
            {
                _search = value;
                _filterSearch.SetWhat(_search); // Задание поисковой строки
                _filterSearch.SetWhere("InventoryNumber"); // Задание пути для поиска
                _filterSearch.AddWhere(_filterSearch.Member); // Добавление пути в список путей
                _filterSearch.SetWhere("SerialNumber"); // Задание второго пути поиска
                _filterSearch.AddWhere(_filterSearch.Member); // Добавление второго пути в список путей
                _filterSearch.CreateFilter(); // Создание фильтра
                OnPropertyChanged("Search");
            }
        }
        public DateTime Date
        {
            get { return _date; }
            set { _date = value; }
        }
        //Свойства команд
        public IDelegateCommand AddServiceLog
        {
            get
            {
                if (_addServiceLog == null)
                {
                    _addServiceLog = new DelegateCommand(OpenDialog, CanExecuteAddServiceLog);
                }
                return _addServiceLog;
            }
        }
        #endregion        
        #region Конструкторы
        public AddServiceLogVM()
        {
            _search = String.Empty;
            _parameter = System.Linq.Expressions.Expression.Parameter(typeof(Device), "d");
            _filterSearch = new FilterSearch(_parameter);
            _filters = new List<IFilter>();
            _filters.Add(_filterSearch);
            _filterSearch.FilterCreated += OnFilterChanged;
            _date = DateTime.Now;

            Devices = new ObservableCollection<Device>();
            ParametersValues = new ObservableCollection<ParameterValue>();
            Services = new ObservableCollection<ServiceForModel>();
            Spares = new ObservableCollection<SpareForModel>();
            Users = new ObservableCollection<Repairer>();

            _selectedServices = new ObservableCollection<ServiceForModel>();
            _selectedSpares = new ObservableCollection<SpareForModel>();

            var sdbContext = SingletonDBContext.GetInstance(new SQLiteContext());
            _dbContext = sdbContext.DBContext;
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Device.Load();
                Devices = context.Device.Local.ToBindingList();
                context.Repairer.Load();
                Users = context.Repairer.Local.ToBindingList();
            }
        }
        #endregion
        #region Методы
        public void OnFilterChanged()
        {
            System.Linq.Expressions.Expression result = null, temp;
            foreach (var item in _filters)
            {
                if (result == null)
                    result = item.GetFilter();
                else
                {
                    temp = item.GetFilter();
                    if (temp != null)
                        result = System.Linq.Expressions.Expression.And(result, temp);
                }
            }
            var lambda = System.Linq.Expressions.Expression.Lambda<Func<Device, bool>>(result, _parameter);
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                Devices = context.Device.Where((Func<Device, bool>)lambda.Compile()).ToList();
            }
        }
        public System.Collections.IList SelectedItems
        {
            set
            {
                System.Collections.IList temp = null;

                temp = ItemsBuilder.SelectItem(value, SelectedServices, typeof(ServiceForModel), SelectedService);
                if (temp != null) SelectedServices = (ObservableCollection<ServiceForModel>)temp;

                temp = ItemsBuilder.SelectItem(value, SelectedSpares, typeof(SpareForModel), SelectedSpare);
                if (temp != null) SelectedSpares = (ObservableCollection<SpareForModel>)temp;

            }
        }
        //Обработчики команд
        private void ExecuteAddServiceLog(object parameter)
        {
            ServiceLog sl = new ServiceLog()
            {
                Date = Date.ToShortDateString(),
                Device = SelectedDevice,
                Repairer = SelectedUser
            };

            sl.ParametersValues = new List<ParameterValue>();
            foreach (var item in ParametersValues)
            {
                sl.ParametersValues.Add(item);
            }
            foreach (var item in SelectedServices)
            {
                sl.ServicesDone.Add(new ServiceDone { RowidServiceForModel = item.Rowid });
            }
            foreach (var item in SelectedSpares)
            {
                sl.SparesUsed.Add(new SpareUsed { RowidSpareForModel = item.Rowid });
            }
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.ServiceLog.Add(sl);
                context.SaveChanges();
            }
        }
        private bool CanExecuteAddServiceLog(object parameter)
        {
            if ((SelectedDevice != null) && (SelectedUser != null) && (SelectedServices.Count > 0))
                if (SelectedServices?.Count > 0)
                    return true;

            return false;
        }
        private async void OpenDialog(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM("Новая запись в журнал", "Вы действительно хотите добавить новую запись в журнал ремонтов?", ExecuteAddServiceLog);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        #endregion
    }
}
