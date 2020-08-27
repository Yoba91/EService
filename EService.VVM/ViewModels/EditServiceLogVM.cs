using EService.BL;
using EService.Data.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EService.VVM.ViewModels
{
    public class EditServiceLogVM : INotifyPropertyChanged
    {
        private DbContext dbContext;
        private ServiceLog serviceLog;
        private DateTime date;
        private string title;

        private ParameterValue selectedParameterValue;
        private SpareForModel selectedSpare;
        private ServiceForModel selectedService;

        private IList<ServiceForModel> selectedServices;
        private IList<SpareForModel> selectedSpares;

        public ObservableCollection<ParameterValue> ParametersValues { get; set; }
        public ObservableCollection<ServiceForModel> Services { get; set; }
        public ObservableCollection<SpareForModel> Spares { get; set; }

        public DateTime Date { get { return date; } set { date = value; } }
        public string Title { get { return title; } set { title = value; } }

        public ParameterValue SelectedParameterValue { get { return selectedParameterValue; } set { selectedParameterValue = value; } }
        public SpareForModel SelectedSpare { get { return selectedSpare; } set { selectedSpare = value; } }
        public ServiceForModel SelectedService { get { return selectedService; } set { selectedService = value; } }

        public ObservableCollection<ServiceForModel> SelectedServices
        {
            get { return (ObservableCollection<ServiceForModel>)selectedServices; }
            set
            {
                selectedServices = value;
                //this.AddServiceLog.RaiseCanExecuteChanged();
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

        public EditServiceLogVM(long rowid)
        {
            ParametersValues = new ObservableCollection<ParameterValue>();
            Services = new ObservableCollection<ServiceForModel>();
            Spares = new ObservableCollection<SpareForModel>();

            var sdbContext = SingletonDBContext.GetInstance(new SQLiteContext());
            dbContext = sdbContext.DBContext;
            if (dbContext is SQLiteContext)
            {
                SQLiteContext context = dbContext as SQLiteContext;
                serviceLog = context.ServiceLog.Where(s => s.Rowid == rowid).SingleOrDefault();
                //serviceLog = context.ServiceLog.ToList().LastOrDefault();

                var parameters = context.ParameterForModel.Where(pfm => pfm.RowidModel == serviceLog.Device.RowidModel).ToList();
                foreach (var item in parameters)
                {
                    var pv = context.ServiceLog.Where(s => s.Device.Rowid == serviceLog.Device.Rowid).ToList().LastOrDefault()?.ParametersValues.ToList().LastOrDefault();
                    if (pv == null)
                    {
                        ParametersValues.Add(new ParameterValue() { RowidParameterForModel = item.Rowid, ParameterForModel = item, Value = item.Parameter.Default });
                    }
                    else
                    {
                        ParametersValues.Add(new ParameterValue() { RowidParameterForModel = item.Rowid, ParameterForModel = item, Value = pv.Value });
                    }
                }
                var tempServices = context.ServiceForModel.Where(s => s.RowidModel == serviceLog.Device.RowidModel).ToList();
                foreach (var item in tempServices)
                {
                    Services.Add(item);
                }
                var tempSpares = context.SpareForModel.Where(s => s.RowidModel == serviceLog.Device.RowidModel).ToList();
                foreach (var item in tempSpares)
                {
                    Spares.Add(item);
                }
            }
            Date = serviceLog.DateTime;
            Title = String.Format("Изменить {0} I/N - \"{1}\" | S/N - \"{2}\"", serviceLog.Device.Model.TypeModel.ShortName, serviceLog.Device.InventoryNumber, serviceLog.Device.SerialNumber);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
