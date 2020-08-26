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

namespace EService.ViewModel
{
    public class AddServiceLogViewModel : INotifyPropertyChanged
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

        public IList<Device> Devices { get { return devices; } set { devices = value; OnPropertyChanged("Devices"); } }
        public IList<ParameterValue> ParametersValues { get; set; }
        public IList<Service> Services { get; set; }
        public IList<Spare> Spares { get; set; }
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
                        ParametersValues = context.ParameterValue.Where(pv => pv.ParameterForModel.Model.Rowid == selectedDevice.RowidModel).ToList();
                        Services = context.ServiceForModel.Where(s => s.RowidModel == selectedDevice.RowidModel).Select(s => s.Service).ToList();
                        Spares = context.SpareForModel.Where(s => s.RowidModel == selectedDevice.RowidModel).Select(s => s.Spare).ToList();
                    }
                }
            } 
        }
        public ParameterValue SelectedParameterValue { get { return selectedParameterValue; } set { selectedParameterValue = value; } }
        public Spare SelectedSpare { get { return selectedSpare; } set { selectedSpare = value; } }
        public Service SelectedService { get { return selectedService; } set { selectedService = value; } }


        public AddServiceLogViewModel()
        {
            search = String.Empty;
            parameter = Expression.Parameter(typeof(ServiceLog), "d");
            filterSearch = new FilterSearch(parameter);
            filters = new List<IFilter>();
            filters.Add(filterSearch);
            filterSearch.FilterCreated += OnFilterChanged;

            Devices = new ObservableCollection<Device>();
            ParametersValues = new ObservableCollection<ParameterValue>();
            Services = new ObservableCollection<Service>();
            Spares = new ObservableCollection<Spare>();

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
