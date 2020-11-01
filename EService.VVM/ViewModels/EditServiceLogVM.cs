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
    public class EditServiceLogVM : BaseVM
    {
        #region Поля
        private DbContext _dbContext;
        private ServiceLog _serviceLog;
        private DateTime _date;
        private string _title;

        private ParameterValue _selectedParameterValue;
        private SpareForModel _selectedSpare;
        private ServiceForModel _selectedService;
        private Repairer _selectedUser;

        private IDelegateCommand _editServiceLog;

        private IList<ServiceForModel> _selectedServices;
        private IList<SpareForModel> _selectedSpares;
        #endregion
        #region Свойства
        //Свойства команд
        public IDelegateCommand EditServiceLog
        {
            get
            {
                if (_editServiceLog == null)
                {
                    _editServiceLog = new DelegateCommand(OpenDialog);
                }
                return _editServiceLog;
            }
        }
        //Свойства модели
        public ObservableCollection<ParameterValue> ParametersValues { get; set; }
        public ObservableCollection<Repairer> Users { get; set; }
        public ObservableCollection<ServiceForModel> Services { get; set; }
        public ObservableCollection<SpareForModel> Spares { get; set; }
        public ObservableCollection<ServiceForModel> OldServices { get; set; }
        public ObservableCollection<SpareForModel> OldSpares { get; set; }
        public DateTime Date { get { return _date; } set { _date = value; } }
        public string Title { get { return _title; } set { _title = value; } }
        public ParameterValue SelectedParameterValue { get { return _selectedParameterValue; } set { _selectedParameterValue = value; } }
        public SpareForModel SelectedSpare { get { return _selectedSpare; } set { _selectedSpare = value; } }
        public ServiceForModel SelectedService { get { return _selectedService; } set { _selectedService = value; } }
        public Repairer SelectedUser { get { return _selectedUser; } set { _selectedUser = value; } }
        public bool NewSpare { get; set; }
        public bool NewService { get; set; }
        public ObservableCollection<ServiceForModel> SelectedServices { get { return (ObservableCollection<ServiceForModel>)_selectedServices; } set { _selectedServices = value; } }
        public ObservableCollection<SpareForModel> SelectedSpares { get { return (ObservableCollection<SpareForModel>)_selectedSpares; } set { _selectedSpares = value; } }
        #endregion
        #region Конструкторы
        public EditServiceLogVM(long rowid)
        {
            ParametersValues = new ObservableCollection<ParameterValue>();
            Services = new ObservableCollection<ServiceForModel>();
            Spares = new ObservableCollection<SpareForModel>();
            Users = new ObservableCollection<Repairer>();
            OldServices = new ObservableCollection<ServiceForModel>();
            OldSpares = new ObservableCollection<SpareForModel>();
            NewSpare = false;
            NewService = false;
            _selectedServices = new ObservableCollection<ServiceForModel>();
            _selectedSpares = new ObservableCollection<SpareForModel>();
            _dbContext = SingletonDBContext.GetInstance(new SQLiteContext()).DBContext;
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                _serviceLog = context.ServiceLog.Where(s => s.Rowid == rowid).SingleOrDefault();

                //var parameters = context.ParameterForModel.Where(pfm => pfm.RowidModel == _serviceLog.Device.RowidModel).ToList();
                //foreach (var item in parameters)
                //{
                //    var pv = context.ServiceLog.Where(s => s.Device.Rowid == _serviceLog.Device.Rowid).ToList().LastOrDefault()?.ParametersValues.ToList().LastOrDefault();
                //    if (pv == null)
                //    {
                //        ParametersValues.Add(new ParameterValue() { RowidParameterForModel = item.Rowid, ParameterForModel = item, Value = item.Parameter.Default });
                //    }
                //    else
                //    {
                //        ParametersValues.Add(new ParameterValue() { RowidParameterForModel = item.Rowid, ParameterForModel = item, Value = pv.Value });
                //    }
                //}
                context.ParameterValue.Where(pv => pv.ServiceLog.Rowid == _serviceLog.Rowid).ToList().ForEach(item => ParametersValues.Add(item));
                List<ParameterForModel> tempPFM = ParametersValues.Select(pv => pv.ParameterForModel).ToList();
                var temp = context.ParameterForModel.Where(pfm => pfm.RowidModel == _serviceLog.Device.RowidModel).ToList();
                temp = temp.Where(item => !tempPFM.Contains(item)).ToList();
                if (temp != null)
                {
                    temp.ForEach(item => ParametersValues.Add(new ParameterValue() { ParameterForModel = item }));
                }
                context.ServiceForModel.Where(s => s.RowidModel == _serviceLog.Device.RowidModel).ToList().ForEach(item => Services.Add(item));                
                context.SpareForModel.Where(s => s.RowidModel == _serviceLog.Device.RowidModel).ToList().ForEach(item => Spares.Add(item));                
                context.Repairer.ToList().ForEach(item => Users.Add(item));
                SelectedUser = _serviceLog.Repairer;
                _serviceLog.SparesUsed.Select(su => su.SpareForModel).ToList().ForEach(item => OldSpares.Add(item));                
                _serviceLog.ServicesDone.Select(sd => sd.ServiceForModel).ToList().ForEach(item => OldServices.Add(item));                
            }
            Date = _serviceLog.DateTime;
            Title = String.Format("Изменить {0} I/N - \"{1}\" | S/N - \"{2}\"", _serviceLog.Device.Model.TypeModel.ShortName, _serviceLog.Device.InventoryNumber, _serviceLog.Device.SerialNumber);
        }
        #endregion
        #region Методы
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
        private void ExecuteEditServiceLog(object parameter)
        {
            var context = _dbContext as SQLiteContext;
            context.Configuration.LazyLoadingEnabled = false;
            context.ServiceLog.Where(s => s.Rowid == _serviceLog.Rowid).SingleOrDefault().Date = Date.ToShortDateString();
            context.ServiceLog.Where(s => s.Rowid == _serviceLog.Rowid).SingleOrDefault().Repairer = SelectedUser;
            var pv = context.ParameterValue.Where(p => p.RowidServiceLog == _serviceLog.Rowid).ToList();
            //foreach (var item in pv)
            //{
            //    context.ParameterValue.Remove(item);
            //}
            //foreach (var item in ParametersValues)
            //{
            //    context.ServiceLog.Where(s => s.Rowid == _serviceLog.Rowid).SingleOrDefault().ParametersValues.Add(item);
            //}
            foreach (var item in ParametersValues)
            {
                var parametersValues = context.ParameterValue.Where(p => p.Rowid == item.Rowid).ToList();
                if (parametersValues.Count == 0)
                {
                    item.ServiceLog = _serviceLog;
                    context.ParameterValue.Add(item);
                }
                else
                { 
                   parametersValues.ForEach(i => i.Value = item.Value);
                }
            }            
            if (NewService)
            {
                var sd = context.ServiceDone.Where(s => s.RowidServiceLog == _serviceLog.Rowid).ToList();
                foreach (var item in sd)
                {
                    context.ServiceDone.Remove(item);
                }
                foreach (var item in _selectedServices)
                {
                    var serviceDone = new ServiceDone
                    {
                        RowidServiceForModel = item.Rowid,
                        RowidServiceLog = _serviceLog.Rowid
                    };
                    context.ServiceLog.Where(s => s.Rowid == _serviceLog.Rowid).SingleOrDefault().ServicesDone.Add(serviceDone);
                }
            }
            if (NewSpare)
            {
                var su = context.SpareUsed.Where(s => s.RowidServiceLog == _serviceLog.Rowid).ToList();
                foreach (var item in su)
                {
                    context.SpareUsed.Remove(item);
                }
                foreach (var item in _selectedSpares)
                {
                    var spareUsed = new SpareUsed
                    {
                        RowidSpareForModel = item.Rowid,
                        RowidServiceLog = _serviceLog.Rowid
                    };
                    context.ServiceLog.Where(s => s.Rowid == _serviceLog.Rowid).SingleOrDefault().SparesUsed.Add(spareUsed);
                }
            }
            context.SaveChanges();
            context.Configuration.LazyLoadingEnabled = true;
            context.Database.Initialize(true);
        }
        private async void OpenDialog(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM(Title, "Вы действительно хотите изменить запись в журнал ремонтов?", ExecuteEditServiceLog);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        #endregion
    }
}
