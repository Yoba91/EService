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
using System.Windows;

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

        private IDelegateCommand editServiceLog;

        private IList<ServiceForModel> selectedServices;
        private IList<SpareForModel> selectedSpares;

        public ObservableCollection<ParameterValue> ParametersValues { get; set; }
        public ObservableCollection<ServiceForModel> Services { get; set; }
        public ObservableCollection<SpareForModel> Spares { get; set; }
        public ObservableCollection<ServiceForModel> OldServices { get; set; }
        public ObservableCollection<SpareForModel> OldSpares { get; set; }

        public DateTime Date { get { return date; } set { date = value; } }
        public string Title { get { return title; } set { title = value; } }

        public ParameterValue SelectedParameterValue { get { return selectedParameterValue; } set { selectedParameterValue = value; } }
        public SpareForModel SelectedSpare { get { return selectedSpare; } set { selectedSpare = value; } }
        public ServiceForModel SelectedService { get { return selectedService; } set { selectedService = value; } }
        public bool NewSpare { get; set; }
        public bool NewService { get; set; }

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

        public IDelegateCommand EditServiceLog
        {
            get
            {
                if (editServiceLog == null)
                {
                    editServiceLog = new DelegateCommand(OpenDialog);
                }
                return editServiceLog;
            }
        }

        private void ExecuteEditServiceLog(object parameter)
        {
            var context = dbContext as SQLiteContext;
            context.Configuration.LazyLoadingEnabled = false;
            context.ServiceLog.Where(s => s.Rowid == serviceLog.Rowid).SingleOrDefault().Date = Date.ToShortDateString();
            var pv = context.ParameterValue.Where(p => p.RowidServiceLog == serviceLog.Rowid).ToList();
            foreach (var item in pv)
            {
                context.ParameterValue.Remove(item);
            }
            foreach (var item in ParametersValues)
            {
                context.ServiceLog.Where(s => s.Rowid == serviceLog.Rowid).SingleOrDefault().ParametersValues.Add(item);
            }
            if (NewService)
            {
                var sd = context.ServiceDone.Where(s => s.RowidServiceLog == serviceLog.Rowid).ToList();
                foreach (var item in sd)
                {
                    context.ServiceDone.Remove(item);
                }
                foreach (var item in selectedServices)
                {
                    var serviceDone = new ServiceDone
                    {
                        RowidServiceForModel = item.Rowid,
                        RowidServiceLog = serviceLog.Rowid
                    };
                    context.ServiceLog.Where(s => s.Rowid == serviceLog.Rowid).SingleOrDefault().ServicesDone.Add(serviceDone);
                }
            }
            if (NewSpare)
            {
                var su = context.SpareUsed.Where(s => s.RowidServiceLog == serviceLog.Rowid).ToList();
                foreach (var item in su)
                {
                    context.SpareUsed.Remove(item);
                }
                foreach (var item in selectedSpares)
                {
                    var spareUsed = new SpareUsed
                    {
                        RowidSpareForModel = item.Rowid,
                        RowidServiceLog = serviceLog.Rowid
                    };
                    context.ServiceLog.Where(s => s.Rowid == serviceLog.Rowid).SingleOrDefault().SparesUsed.Add(spareUsed);
                }
            }
            context.SaveChanges();
        }
        private async void OpenDialog(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM(Title, "Вы действительно хотите изменить запись в журнал ремонтов?", ExecuteEditServiceLog);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }

        public EditServiceLogVM(long rowid)
        {
            ParametersValues = new ObservableCollection<ParameterValue>();
            Services = new ObservableCollection<ServiceForModel>();
            Spares = new ObservableCollection<SpareForModel>();
            OldServices = new ObservableCollection<ServiceForModel>();
            OldSpares = new ObservableCollection<SpareForModel>();
            NewSpare = false;
            NewService = false;
            selectedServices = new ObservableCollection<ServiceForModel>();
            selectedSpares = new ObservableCollection<SpareForModel>();
            dbContext = SingletonDBContext.GetInstance(new SQLiteContext()).DBContext;
            if (dbContext is SQLiteContext)
            {
                SQLiteContext context = dbContext as SQLiteContext;
                serviceLog = context.ServiceLog.Where(s => s.Rowid == rowid).SingleOrDefault();

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
                var oldSpares = serviceLog.SparesUsed.Select(su => su.SpareForModel).ToList();
                foreach (var item in oldSpares)
                {
                    OldSpares.Add(item);
                }
                var oldServices = serviceLog.ServicesDone.Select(sd => sd.ServiceForModel).ToList();
                foreach (var item in oldServices)
                {
                    OldServices.Add(item);
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
