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
        private string search = String.Empty;

        private DateTime startDate;

        private DateTime endDate;

        private ServiceLog selectedServiceLog;

        private IList<ServiceLog> serviceLogs;

        private IList<Status> selectedStatuses;
        public String Search { get { return search; } set { search = value; Filter(); OnPropertyChanged("Search"); } }
        public DateTime FirstDate { get { return startDate; } set { startDate = value; Filter(); } }
        public DateTime SecondDate { get { return endDate; } set { endDate = value; Filter(); } }
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
                selectedStatuses = value; Filter(); OnPropertyChanged("SelectedStatuses");
            }
        }

        public ServiceLog SelectedServiceLog { get { return selectedServiceLog; } set 
            { 
                selectedServiceLog = value;
                if (selectedServiceLog != null)
                {
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
            ParameterExpression parameter = Expression.Parameter(typeof(ServiceLog), "s");
            FilterContains<ServiceLog> filterSearch = new FilterContains<ServiceLog>(parameter);
            FilterDate<ServiceLog> filterDate = new FilterDate<ServiceLog>(parameter);
            FilterData<ServiceLog> filter = new FilterData<ServiceLog>(parameter);
            filterSearch.SearchString = Search;
            filterDate.Start = FirstDate;
            filterDate.End = SecondDate;

            IList<Expression> exp = new List<Expression>();            
            exp.Add(filterSearch.CreateFilter("Device","InventoryNumber"));
            exp.Add(filterSearch.CreateFilter("Device", "SerialNumber"));
            var result = FilterContains<ServiceLog>.Or(exp);

            exp = new List<Expression>();
            exp.Add(filterDate.CreateFilter("DateTime"));
            exp.Add(result);
            result = FilterDate<ServiceLog>.And(exp);

            if (SelectedStatuses != null && SelectedStatuses.Count > 0)
            {
                exp = new List<Expression>();
                foreach (var item in SelectedStatuses)
                {
                    filter.RowId = item.Rowid;
                    exp.Add(filter.CreateFilter("Device", "Status", "Rowid"));
                }
                exp.Add(result);
                result = FilterData<ServiceLog>.Or(exp);
            }

            var lambda = FilterContains<ServiceLog>.GetLambda(result, parameter);

            DbContext dbContext = new SQLiteContext();
            if (dbContext is SQLiteContext)
            {
                SQLiteContext context = dbContext as SQLiteContext;                
                ServiceLogs = context.ServiceLog.Where((Func<ServiceLog, bool>)lambda.Compile()).ToList();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public void OnFilterChanged()
        { }
    }
}
