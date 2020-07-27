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
    public class MainViewModel : INotifyPropertyChanged
    {
        private string search;

        private ServiceLog selectedServiceLog;

        private IList<ServiceLog> serviceLogs;

        private IList<Status> selectedStatuses;
        public String Search { get { return search; } set { search = value; Filter(); OnPropertyChanged("Search"); } }
        public DateTime FirstDate { get; set; }
        public DateTime SecondDate { get; set; }
        public ObservableCollection<ParameterValue> ParametersValues { get; set; }
        public ObservableCollection<ServiceDone> ServicesDone { get; set; }
        public ObservableCollection<SpareUsed> SparesUsed { get; set; }
        public IList<ServiceLog> ServiceLogs { get { return serviceLogs; } set { serviceLogs = value; OnPropertyChanged("ServiceLogs"); } }
        public IList<Status> Statuses { get; set; }
        public IList<Repairer> Repairers { get; set; }
        public IList<Dept> Depts { get; set; }
        public IList<TypeModel> TypesModel { get; set; }
        public IList<Model> Models { get; set; }
        public IList<Service> Services { get; set; }
        public IList<Spare> Spares { get; set; }

        public IList<Status> SelectedStatuses { get { return selectedStatuses; } set 
            {

            }
        }

        public ServiceLog SelectedServiceLog { get { return selectedServiceLog; } set 
            { 
                selectedServiceLog = value;
                ParametersValues.Clear();
                ServicesDone.Clear();
                SparesUsed.Clear();
                foreach (var item in selectedServiceLog.ParametersValues)
                {
                    ParametersValues.Add(item);
                }
                foreach (var item in selectedServiceLog.ServicesDone)
                {
                    ServicesDone.Add(item);
                }
                foreach (var item in selectedServiceLog.SparesUsed)
                {
                    SparesUsed.Add(item);
                }
                OnPropertyChanged("SelectedServiceLog"); 
            } 
        }

        public MainViewModel()
        {
            FirstDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            SecondDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            Spares = new ObservableCollection<Spare>();
            Services = new ObservableCollection<Service>();
            Models = new ObservableCollection<Model>();
            TypesModel = new ObservableCollection<TypeModel>();
            Depts = new ObservableCollection<Dept>();
            Repairers = new ObservableCollection<Repairer>();
            Statuses = new ObservableCollection<Status>();
            ServiceLogs = new ObservableCollection<ServiceLog>();
            ParametersValues = new ObservableCollection<ParameterValue>();
            ServicesDone = new ObservableCollection<ServiceDone>();
            SparesUsed = new ObservableCollection<SpareUsed>();
            DbContext dbContext = new SQLiteContext();
            if (dbContext is SQLiteContext)
            {
                SQLiteContext context = dbContext as SQLiteContext;
                context.ServiceLog.Load();
                ServiceLogs = context.ServiceLog.Local.ToBindingList();
                context.Status.Load();
                Statuses = context.Status.Local.ToBindingList();
                context.Repairer.Load();
                Repairers = context.Repairer.Local.ToBindingList();
                context.Dept.Load();
                Depts = context.Dept.Local.ToBindingList();
                context.TypeModel.Load();
                TypesModel = context.TypeModel.Local.ToBindingList();
                context.Model.Load();
                Models = context.Model.Local.ToBindingList();
                context.Service.Load();
                Services = context.Service.Local.ToBindingList();
                context.Spare.Load();
                Spares = context.Spare.Local.ToBindingList();
            }
        }

        public void Filter()
        {
            FilterContains<ServiceLog> filter = new FilterContains<ServiceLog>();
            IList <Expression> exp = new List<Expression>();
            filter.Search = Search;
            exp.Add(filter.CreateFilter("Device","InventoryNumber"));
            exp.Add(filter.CreateFilter("Device", "SerialNumber"));
            var result = filter.Or(exp);
            var lambda = filter.GetLambda(result);

            //var param = Expression.Parameter(typeof(ServiceLog), "s");
            //var dev = Expression.PropertyOrField(param, "Device");
            //var inv = Expression.PropertyOrField(dev, "InventoryNumber");
            //PropertyInfo a = typeof(ServiceLog).GetProperty("Device");
            //PropertyInfo b = a.PropertyType.GetProperty("InventoryNumber");
            //MethodInfo c = b.PropertyType.GetMethod("Contains");
            //var condition = Expression.Call(inv, inv.Type.GetMethod("Contains"), Expression.Constant(search));
            //var l = Expression.Lambda<Func<ServiceLog, bool>>(condition, param);

            DbContext dbContext = new SQLiteContext();
            if (dbContext is SQLiteContext)
            {
                SQLiteContext context = dbContext as SQLiteContext;                
                //context.ServiceLog.Where(s => s.Device.InventoryNumber.Contains(Search)).Load();
                ServiceLogs = context.ServiceLog.Where((Func<ServiceLog, bool>)lambda.Compile()).ToList();
            }
            
            
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
