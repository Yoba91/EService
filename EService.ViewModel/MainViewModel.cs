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

        private FilterSearch filterSearch;
        private FilterDate filterDate;
        private FilterId filterStatus, filterRepairer, filterDept, filterTypeModel, filterModel;
        private List<IFilter> filters;
        ParameterExpression parameter;

        private Status selectedStatus;
        private Repairer selectedRepairer;
        private Dept selectedDept;
        private TypeModel selectedTypeModel;
        private Model selectedModel;
        private Spare selectedSpare;
        private Service selectedService;

        private DateTime startDate;

        private DateTime endDate;

        private ServiceLog selectedServiceLog;

        private IList<ServiceLog> serviceLogs;

        private IList<Status> selectedStatuses;
        private IList<Repairer> selectedRepairers;
        private IList<Dept> selectedDepts;
        private IList<TypeModel> selectedTypesModel;
        private IList<Model> selectedModels;
        private IList<Spare> selectedSpares;
        private IList<Service> selectedServices;

        public Status SelectedStatus { get { return selectedStatus; } set { selectedStatus = value; OnPropertyChanged("SelectedStatus"); } }
        public Repairer SelectedRepairer { get { return selectedRepairer; } set { selectedRepairer = value; OnPropertyChanged("SelectedRepairer"); } }
        public Dept SelectedDept { get { return selectedDept; } set { selectedDept = value; OnPropertyChanged("SelectedDept"); } }
        public TypeModel SelectedTypeModel { get { return selectedTypeModel; } set { selectedTypeModel = value; OnPropertyChanged("SelectedTypeModel"); } }
        public Model SelectedModel { get { return selectedModel; } set { selectedModel = value; OnPropertyChanged("SelectedModel"); } }
        public Spare SelectedSpare { get { return selectedSpare; } set { selectedSpare = value; OnPropertyChanged("SelectedSpare"); } }
        public Service SelectedService { get { return selectedService; } set { selectedService = value; OnPropertyChanged("SelectedService"); } }
        public String Search { get { return search; } 
            set 
            { 
                search = value;
                filterSearch.SetWhat(search);
                filterSearch.SetWhere("Device", "InventoryNumber");
                filterSearch.AddWhere(filterSearch.Member);
                filterSearch.SetWhere("Device", "SerialNumber");
                filterSearch.AddWhere(filterSearch.Member);
                filterSearch.CreateFilter();
                OnPropertyChanged("Search"); 
            } 
        }
        public DateTime FirstDate { get { return startDate; } 
            set 
            { 
                startDate = value;
                filterDate.SetWhat(String.Format("{0}.{1}.{2}", startDate.Day, startDate.Month, startDate.Year), String.Format("{0}.{1}.{2}", endDate.Day, endDate.Month, endDate.Year));
                filterDate.SetWhere("DateTime");
                filterDate.CreateFilter();
            } 
        }
        public DateTime SecondDate { get { return endDate; } 
            set 
            { 
                endDate = value;
                filterDate.SetWhat(String.Format("{0}.{1}.{2}", startDate.Day, startDate.Month, startDate.Year), String.Format("{0}.{1}.{2}", endDate.Day, endDate.Month, endDate.Year));
                filterDate.SetWhere("DateTime");
                filterDate.CreateFilter();
            } 
        }
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

        public System.Collections.IList SelectedItems
        {            
            set
            {
                System.Collections.IList temp = null;

                temp = ItemsBuilder.SelectItem(value, SelectedStatuses, typeof(Status), SelectedStatus);
                if (temp != null) SelectedStatuses = (ObservableCollection<Status>)temp;

                temp = ItemsBuilder.SelectItem(value, SelectedRepairers, typeof(Repairer), SelectedRepairer);
                if (temp != null) SelectedRepairers = (ObservableCollection<Repairer>)temp;

                temp = ItemsBuilder.SelectItem(value, SelectedDepts, typeof(Dept), SelectedDept);
                if (temp != null) SelectedDepts = (ObservableCollection<Dept>)temp;

                temp = ItemsBuilder.SelectItem(value, SelectedTypesModel, typeof(TypeModel), selectedTypeModel);
                if (temp != null) SelectedTypesModel = (ObservableCollection<TypeModel>)temp;

                temp = ItemsBuilder.SelectItem(value, SelectedModels, typeof(Model), SelectedModel);
                if (temp != null) SelectedModels = (ObservableCollection<Model>)temp;

                temp = ItemsBuilder.SelectItem(value, SelectedSpares, typeof(Spare), SelectedSpare);
                if (temp != null) SelectedSpares = (ObservableCollection<Spare>)temp;

                temp = ItemsBuilder.SelectItem(value, SelectedServices, typeof(Service), SelectedService);
                if (temp != null) SelectedServices = (ObservableCollection<Service>)temp;

                OnFilterChanged();
            }
        }

        private void SetFilter<T>(ObservableCollection<T> list, IFilter filter, params string[] parameters) where T:IIdentifier
        {
            List<string> indeses = new List<string>();
            foreach (var item in list)
            {
                indeses.Add(item.Rowid.ToString());
            }
            filter.SetWhat(indeses.ToArray());
            filter.SetWhere(parameters);
            filter.CreateFilter();
        }

        public ObservableCollection<Status> SelectedStatuses
        {
            get { return (ObservableCollection<Status>)selectedStatuses; }
            set
            {
                selectedStatuses = value;
                SetFilter(SelectedStatuses, filterStatus, "Device", "Status", "Rowid");
                OnPropertyChanged("SelectedStatuses");
            }
        }

        public ObservableCollection<Repairer> SelectedRepairers
        {
            get { return (ObservableCollection<Repairer>)selectedRepairers; }
            set
            {
                selectedRepairers = value;
                SetFilter(SelectedRepairers, filterRepairer, "Repairer", "Rowid");
                OnPropertyChanged("SelectedRepairers");
            }
        }

        public ObservableCollection<Dept> SelectedDepts
        {
            get { return (ObservableCollection<Dept>)selectedDepts; }
            set
            {
                selectedDepts = value;
                SetFilter(SelectedDepts, filterDept, "Device", "Dept", "Rowid");
                OnPropertyChanged("SelectedDepts");
            }
        }

        public ObservableCollection<TypeModel> SelectedTypesModel
        {
            get { return (ObservableCollection<TypeModel>)selectedTypesModel; }
            set
            {
                selectedTypesModel = value;
                SetFilter(SelectedTypesModel, filterTypeModel, "Device", "Model", "TypeModel", "Rowid");
                OnPropertyChanged("SelectedTypesModel");
            }
        }

        public ObservableCollection<Model> SelectedModels
        {
            get { return (ObservableCollection<Model>)selectedModels; }
            set
            {
                selectedModels = value;
                SetFilter(SelectedModels, filterModel, "Device", "Model", "Rowid");
                OnPropertyChanged("SelectedModels");
            }
        }

        public ObservableCollection<Spare> SelectedSpares
        {
            get { return (ObservableCollection<Spare>)selectedSpares; }
            set
            {
                selectedSpares = value; OnPropertyChanged("SelectedSpares");
            }
        }

        public ObservableCollection<Service> SelectedServices
        {
            get { return (ObservableCollection<Service>)selectedServices; }
            set
            {
                selectedServices = value; OnPropertyChanged("SelectedServices");
            }
        }

        public ServiceLog SelectedServiceLog
        {
            get { return selectedServiceLog; }
            set
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
            parameter = Expression.Parameter(typeof(ServiceLog), "s");
            filterDate = new FilterDate(parameter);
            filterSearch = new FilterSearch(parameter);
            filterStatus = new FilterId(parameter);
            filterDept = new FilterId(parameter);
            filterRepairer = new FilterId(parameter);
            filterTypeModel = new FilterId(parameter);
            filterModel = new FilterId(parameter);
            filters = new List<IFilter>();

            filters.Add(filterDate);
            filters.Add(filterSearch);
            filters.Add(filterStatus);
            filters.Add(filterRepairer);
            filters.Add(filterDept);
            filters.Add(filterTypeModel);
            filters.Add(filterModel);

            filterDate.FilterCreated += OnFilterChanged;
            filterSearch.FilterCreated += OnFilterChanged;

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
            SelectedStatuses = new ObservableCollection<Status>();
            SelectedRepairers = new ObservableCollection<Repairer>();
            SelectedDepts = new ObservableCollection<Dept>();
            SelectedTypesModel = new ObservableCollection<TypeModel>();
            SelectedModels = new ObservableCollection<Model>();
            SelectedSpares = new ObservableCollection<Spare>();
            SelectedServices = new ObservableCollection<Service>();

            

            DbContext dbContext = new SQLiteContext();
            if (dbContext is SQLiteContext)
            {
                SQLiteContext context = dbContext as SQLiteContext;
                //context.ServiceLog.Load();
                //ServiceLogs = context.ServiceLog.Local.ToBindingList();
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
                    if(temp!=null)
                    result = Expression.And(result, temp);
                }
            }
            var lambda = FilterContains<ServiceLog>.GetLambda(result, parameter);
            DbContext dbContext = new SQLiteContext();
            if (dbContext is SQLiteContext)
            {
                SQLiteContext context = dbContext as SQLiteContext;
                ServiceLogs = context.ServiceLog.Where((Func<ServiceLog, bool>)lambda.Compile()).ToList();
            }
        }
    }
}
