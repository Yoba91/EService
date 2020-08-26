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

namespace EService.VVM.ViewModels
{
    public class AddServiceLogVM : INotifyPropertyChanged
    {
        private string search;
        private FilterSearch filterSearch;
        ParameterExpression parameter;
        private IList<IFilter> filters;

        private DateTime date;

        private IList<Device> devices;

        private Device selectedDevice;
        private ParameterValue selectedParameterValue;
        private Spare selectedSpare;
        private Service selectedService;

        private IList<Service> selectedServices;
        private IList<Spare> selectedSpares;

        public ObservableCollection<Service> SelectedServices
        {
            get { return (ObservableCollection<Service>)selectedServices; }
            set
            {
                selectedServices = value;
            }
        }
        public ObservableCollection<Spare> SelectedSpares
        {
            get { return (ObservableCollection<Spare>)selectedSpares; }
            set
            {
                selectedSpares = value;
            }
        }
        public IList<Device> Devices { get { return devices; } set { devices = value; OnPropertyChanged("Devices"); } }
        public ObservableCollection<ParameterValue> ParametersValues { get; set; }
        public ObservableCollection<Service> Services { get; set; }
        public ObservableCollection<Spare> Spares { get; set; }
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
                    DbContext dbContext = new SQLiteContext();
                    if (dbContext is SQLiteContext)
                    {
                        SQLiteContext context = dbContext as SQLiteContext;
                        var parameters = context.ParameterForModel.Where(pfm => pfm.RowidModel == selectedDevice.RowidModel).ToList();
                        foreach (var item in parameters)
                        {
                            var pv = context.ServiceLog.Where(s => s.Device.Rowid == SelectedDevice.Rowid).ToList().LastOrDefault()?.ParametersValues.ToList().LastOrDefault();
                            if(pv == null)
                            {
                                ParametersValues.Add(new ParameterValue() { RowidParameterForModel = item.Rowid, ParameterForModel = item, Value = item.Parameter.Default });
                            }
                            else
                            {
                                ParametersValues.Add(pv);
                            }
                        }
                        var tempServices = context.ServiceForModel.Where(s => s.RowidModel == selectedDevice.RowidModel).Select(s => s.Service).ToList();
                        foreach (var item in tempServices)
                        {
                            Services.Add(item);
                        }
                        var tempSpares = context.SpareForModel.Where(s => s.RowidModel == selectedDevice.RowidModel).Select(s => s.Spare).ToList();
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

                temp = ItemsBuilder.SelectItem(value, SelectedServices, typeof(Service), SelectedService);
                if (temp != null) SelectedServices = (ObservableCollection<Service>)temp;

                temp = ItemsBuilder.SelectItem(value, SelectedSpares, typeof(Spare), SelectedSpare);
                if (temp != null) SelectedSpares = (ObservableCollection<Spare>)temp;

            }
        }

        public ParameterValue SelectedParameterValue 
        {
            get { return selectedParameterValue; } 
            set { selectedParameterValue = value; } 
        }
        public Spare SelectedSpare { get { return selectedSpare; } set { selectedSpare = value; } }
        public Service SelectedService { get { return selectedService; } set { selectedService = value; } }
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

        public AddServiceLogVM()
        {
            search = String.Empty;
            parameter = Expression.Parameter(typeof(Device), "d");
            filterSearch = new FilterSearch(parameter);
            filters = new List<IFilter>();
            filters.Add(filterSearch);
            filterSearch.FilterCreated += OnFilterChanged;
            date = DateTime.Now;

            Devices = new ObservableCollection<Device>();
            ParametersValues = new ObservableCollection<ParameterValue>();
            Services = new ObservableCollection<Service>();
            Spares = new ObservableCollection<Spare>();

            selectedServices = new ObservableCollection<Service>();
            selectedSpares = new ObservableCollection<Spare>();

            DbContext dbContext = new SQLiteContext();
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
            Expression result = null, temp;
            foreach (var item in filters)
            {
                if (result == null)
                    result = item.GetFilter();
                else
                {
                    temp = item.GetFilter();
                    if (temp != null)
                        result = Expression.And(result, temp);
                }
            }
            var lambda = Expression.Lambda<Func<Device, bool>>(result, parameter);
            DbContext dbContext = new SQLiteContext();
            if (dbContext is SQLiteContext)
            {
                SQLiteContext context = dbContext as SQLiteContext;
                Devices = context.Device.Where((Func<Device, bool>)lambda.Compile()).ToList();
            }
        }
    }
}
