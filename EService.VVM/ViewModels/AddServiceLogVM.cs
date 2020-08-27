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
    public class AddServiceLogVM : INotifyPropertyChanged
    {
        private DbContext dbContext;
        private string search;
        private FilterSearch filterSearch;
        ParameterExpression parameter;
        private IList<IFilter> filters;

        private DateTime date;

        private IList<Device> devices;

        private Device selectedDevice;
        private ParameterValue selectedParameterValue;
        private SpareForModel selectedSpare;
        private ServiceForModel selectedService;

        private IList<ServiceForModel> selectedServices;
        private IList<SpareForModel> selectedSpares;

        private IDelegateCommand addServiceLog;

        public ObservableCollection<ServiceForModel> SelectedServices
        {
            get { return (ObservableCollection<ServiceForModel>)selectedServices; }
            set
            {
                selectedServices = value;
                this.AddServiceLog.RaiseCanExecuteChanged();
            }
        }
        public ObservableCollection<SpareForModel> SelectedSpares
        {
            get { return (ObservableCollection<SpareForModel>)selectedSpares; }
            set
            {
                selectedSpares = value;
            }
        }
        public IList<Device> Devices { get { return devices; } set { devices = value; OnPropertyChanged("Devices"); } }
        public ObservableCollection<ParameterValue> ParametersValues { get; set; }
        public ObservableCollection<ServiceForModel> Services { get; set; }
        public ObservableCollection<SpareForModel> Spares { get; set; }
        public Device SelectedDevice
        {
            get { return selectedDevice; }
            set
            {
                selectedDevice = value;
                if (selectedDevice != null)
                {
                    ParametersValues.Clear();
                    Services.Clear();
                    Spares.Clear();
                    if (dbContext is SQLiteContext)
                    {
                        SQLiteContext context = dbContext as SQLiteContext;
                        var parameters = context.ParameterForModel.Where(pfm => pfm.RowidModel == selectedDevice.RowidModel).ToList();
                        foreach (var item in parameters)
                        {
                            var pv = context.ServiceLog.Where(s => s.Device.Rowid == SelectedDevice.Rowid).ToList().LastOrDefault()?.ParametersValues.ToList().LastOrDefault();
                            if (pv == null)
                            {
                                ParametersValues.Add(new ParameterValue() { RowidParameterForModel = item.Rowid, ParameterForModel = item, Value = item.Parameter.Default });
                            }
                            else
                            {
                                ParametersValues.Add(new ParameterValue() { RowidParameterForModel = item.Rowid, ParameterForModel = item, Value = pv.Value });
                            }
                        }
                        var tempServices = context.ServiceForModel.Where(s => s.RowidModel == selectedDevice.RowidModel).ToList();
                        foreach (var item in tempServices)
                        {
                            Services.Add(item);
                        }
                        var tempSpares = context.SpareForModel.Where(s => s.RowidModel == selectedDevice.RowidModel).ToList();
                        foreach (var item in tempSpares)
                        {
                            Spares.Add(item);
                        }
                    }
                }
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

        public ParameterValue SelectedParameterValue
        {
            get { return selectedParameterValue; }
            set { selectedParameterValue = value; }
        }
        public SpareForModel SelectedSpare { get { return selectedSpare; } set { selectedSpare = value; } }
        public ServiceForModel SelectedService { get { return selectedService; } set { selectedService = value; } }
        public String Search
        {
            get { return search; }
            set
            {
                search = value;
                filterSearch.SetWhat(search); // Задание поисковой строки
                filterSearch.SetWhere("InventoryNumber"); // Задание пути для поиска
                filterSearch.AddWhere(filterSearch.Member); // Добавление пути в список путей
                filterSearch.SetWhere("SerialNumber"); // Задание второго пути поиска
                filterSearch.AddWhere(filterSearch.Member); // Добавление второго пути в список путей
                filterSearch.CreateFilter(); // Создание фильтра
                OnPropertyChanged("Search");
            }
        }
        public DateTime Date
        {
            get { return date; }
            set { date = value; }
        }

        public IDelegateCommand AddServiceLog
        {
            get
            {
                if (addServiceLog == null)
                {
                    addServiceLog = new DelegateCommand(OpenDialog, CanExecuteAddServiceLog);
                }
                return addServiceLog;
            }
        }

        private void ExecuteAddServiceLog(object parameter)
        {
            ServiceLog sl = new ServiceLog()
            {
                Date = Date.ToShortDateString(),
                RowidDevice = SelectedDevice.Rowid,
                RowidRepairer = 1,              //ЗАГЛУШКА TODO Авторизация 
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
            if (dbContext is SQLiteContext)
            {
                SQLiteContext context = dbContext as SQLiteContext;
                context.ServiceLog.Add(sl);
                context.SaveChanges();
            }
        }

        private bool CanExecuteAddServiceLog(object parameter)
        {
            if (SelectedDevice != null)
                if (SelectedServices?.Count > 0)
                    return true;

            return false;
        }

        private async void OpenDialog(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM("Новая запись в журнал","Вы действительно хотите добавить новую запись в журнал ремонтов?",ExecuteAddServiceLog);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }

        public AddServiceLogVM()
        {
            search = String.Empty;
            parameter = System.Linq.Expressions.Expression.Parameter(typeof(Device), "d");
            filterSearch = new FilterSearch(parameter);
            filters = new List<IFilter>();
            filters.Add(filterSearch);
            filterSearch.FilterCreated += OnFilterChanged;
            date = DateTime.Now;

            Devices = new ObservableCollection<Device>();
            ParametersValues = new ObservableCollection<ParameterValue>();
            Services = new ObservableCollection<ServiceForModel>();
            Spares = new ObservableCollection<SpareForModel>();

            selectedServices = new ObservableCollection<ServiceForModel>();
            selectedSpares = new ObservableCollection<SpareForModel>();

            var sdbContext = SingletonDBContext.GetInstance(new SQLiteContext());
            dbContext = sdbContext.DBContext;
            if (dbContext is SQLiteContext)
            {
                SQLiteContext context = dbContext as SQLiteContext;
                context.Device.Load();
                Devices = context.Device.Local.ToBindingList();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public void OnFilterChanged()
        {
            System.Linq.Expressions.Expression result = null, temp;
            foreach (var item in filters)
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
            var lambda = System.Linq.Expressions.Expression.Lambda<Func<Device, bool>>(result, parameter);
            if (dbContext is SQLiteContext)
            {
                SQLiteContext context = dbContext as SQLiteContext;
                Devices = context.Device.Where((Func<Device, bool>)lambda.Compile()).ToList();
            }
        }
    }
}
