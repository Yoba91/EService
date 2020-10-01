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
using System.Windows.Input;
using System.Security.Principal;

namespace EService.VVM.ViewModels
{
    public class ServiceLogVM : INotifyPropertyChanged
    {
        //Поля модели представления
        private DbContext dbContext;
        private string search = String.Empty; //Поисковая строка

        private FilterSearch filterSearch; //Фильтр поиска
        private FilterDate filterDate; //Фильтр даты
        private FilterId filterStatus, filterRepairer, filterDept, filterTypeModel, filterModel; //Фильтры по ID
        private FilterIdAnd filterSparesUsed, filterServicesDone; //Вложенные фильтры по ID
        private List<IFilter> filters; //Список всех фильтров
        ParameterExpression parameter, parameterSU, parameterSD; //Параметры для формирования лямбды фильтрации     

        private DateTime startDate; //Начальная дата
        private DateTime endDate; //Конечная дата
        private DateTime reservedStartDate; //Зарезервированная начальная дата
        private DateTime reservedEndDate; //Зарезервированная конечная дата
        private bool allTime; //Триггер для поиска за всё время
        private bool reverseAllTime; //Блокировщик выбора даты

        private ServiceLog selectedServiceLog; //Выбранная запись сервисного журнала
        private Status selectedStatus; //Выбранный статус
        private Repairer selectedRepairer; //Выбранный исполнитель ремонта
        private Dept selectedDept; //Выбранный отдел
        private TypeModel selectedTypeModel; //Выбранный тип устройства
        private Model selectedModel; //Выбранная модель
        private Spare selectedSpare; //Выбранная запчасть
        private Service selectedService; //Выбранный вид обслуживания

        private IList<ServiceLog> serviceLogs; //Сервисный журнал

        private IList<Status> selectedStatuses; //Список выбранных статусов
        private IList<Repairer> selectedRepairers; //Список Выбранных исполнителей
        private IList<Dept> selectedDepts; //Список выбранных отделов
        private IList<TypeModel> selectedTypesModel; //Список выбранных типов устройств
        private IList<Model> selectedModels; //Список выбранных моделей устройств
        private IList<Spare> selectedSpares; //Список выбранных запчастей
        private IList<Service> selectedServices; //Список выбранных видов обслуживания

        private IDelegateCommand openAddServiceLogWindow; //Команда открытия окна добавления записи в журнал
        private IDelegateCommand openEditServiceLogWindow; //Команда открытия окна изменения записи в журнале
        private IDelegateCommand refreshServiceLogWindow; //Команда обновления данных в окне
        private IDelegateCommand openDialogWindow; //Команда открытия диалогового окна

        //Команды для кнопок
        public IDelegateCommand AddServiceLogCommand
        {
            get
            {
                if (openAddServiceLogWindow == null)
                {
                    openAddServiceLogWindow = new OpenWindowCommand(ExecuteAddServiceLog, this);
                }
                return openAddServiceLogWindow;
            }
        }
        public IDelegateCommand EditServiceLogCommand
        {
            get
            {
                if (openEditServiceLogWindow == null)
                {
                    openEditServiceLogWindow = new OpenWindowCommand(ExecuteEditServiceLog, CanExecuteEditServiceLog, this);
                }
                return openEditServiceLogWindow;
            }
        }
        public IDelegateCommand RemoveServiceLogCommand
        {
            get
            {
                if (openDialogWindow == null)
                {
                    openDialogWindow = new OpenWindowCommand(OpenDialog, CanExecuteEditServiceLog, this);
                }
                return openDialogWindow;
            }
        }
        public IDelegateCommand RefreshServiceLogCommand
        {
            get
            {
                if (refreshServiceLogWindow == null)
                {
                    refreshServiceLogWindow = new DelegateCommand(ExecuteRefreshServiceLog);
                }
                return refreshServiceLogWindow;
            }
        }

        //Обработчики для команд
        private void ExecuteAddServiceLog(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var addServiceLogVM = new AddServiceLogVM();
            displayRootRegistry.ShowPresentation(addServiceLogVM);
        }

        private async void ExecuteEditServiceLog(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var editServiceLogVM = new EditServiceLogVM(selectedServiceLog.Rowid);
            await displayRootRegistry.ShowModalPresentation(editServiceLogVM);
        }
        private void ExecuteRemoveServiceLog(object parameter)
        {
            if (dbContext is SQLiteContext)
            {
                SQLiteContext context = dbContext as SQLiteContext;
                context.Configuration.LazyLoadingEnabled = false;
                context.ServiceLog.Remove(selectedServiceLog);
                context.SaveChanges();
                SelectedServiceLog = null;
                OnFilterChanged();
            }
        }
        private void ExecuteRefreshServiceLog(object parameter)
        {
            OnFilterChanged();
        }
        private async void OpenDialog(object parameter)
        {
            var message = String.Format("Вы действительно хотите удалить запись ремонта {0} I/N - \"{1}\" | S/N - \"{2}\"?", selectedServiceLog.Device.Model.TypeModel.ShortName, selectedServiceLog.Device.InventoryNumber, selectedServiceLog.Device.SerialNumber);
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM("Удаление записи", message, ExecuteRemoveServiceLog);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        private bool CanExecuteEditServiceLog(object parameter)
        {
            if (selectedServiceLog != null)
                return true;
            return false;
        }

        //Свойства модели
        public Status SelectedStatus { get { return selectedStatus; } set { selectedStatus = value; OnPropertyChanged("SelectedStatus"); } }
        public Repairer SelectedRepairer { get { return selectedRepairer; } set { selectedRepairer = value; OnPropertyChanged("SelectedRepairer"); } }
        public Dept SelectedDept { get { return selectedDept; } set { selectedDept = value; OnPropertyChanged("SelectedDept"); } }
        public TypeModel SelectedTypeModel { get { return selectedTypeModel; } set { selectedTypeModel = value; OnPropertyChanged("SelectedTypeModel"); } }
        public Model SelectedModel { get { return selectedModel; } set { selectedModel = value; OnPropertyChanged("SelectedModel"); } }
        public Spare SelectedSpare { get { return selectedSpare; } set { selectedSpare = value; OnPropertyChanged("SelectedSpare"); } }
        public Service SelectedService { get { return selectedService; } set { selectedService = value; OnPropertyChanged("SelectedService"); } }
        public String Search
        {
            get { return search; }
            set
            {
                search = value;
                filterSearch.SetWhat(search); // Задание поисковой строки
                filterSearch.SetWhere("Device", "InventoryNumber"); // Задание пути для поиска
                filterSearch.AddWhere(filterSearch.Member); // Добавление пути в список путей
                filterSearch.SetWhere("Device", "SerialNumber"); // Задание второго пути поиска
                filterSearch.AddWhere(filterSearch.Member); // Добавление второго пути в список путей
                filterSearch.CreateFilter(); // Создание фильтра
                OnPropertyChanged("Search");
            }
        }
        public DateTime FirstDate
        {
            get { return startDate; }
            set
            {
                startDate = value;
                filterDate.SetWhat(String.Format("{0}.{1}.{2}", startDate.Day, startDate.Month, startDate.Year), String.Format("{0}.{1}.{2}", endDate.Day, endDate.Month, endDate.Year));
                filterDate.SetWhere("DateTime");
                filterDate.CreateFilter();
            }
        }
        public DateTime SecondDate
        {
            get { return endDate; }
            set
            {
                endDate = value;
                filterDate.SetWhat(String.Format("{0}.{1}.{2}", startDate.Day, startDate.Month, startDate.Year), String.Format("{0}.{1}.{2}", endDate.Day, endDate.Month, endDate.Year));
                filterDate.SetWhere("DateTime");
                filterDate.CreateFilter();
            }
        }
        public bool ReverseAllTime
        {
            get { return reverseAllTime; }
            set { reverseAllTime = value; OnPropertyChanged("ReverseAllTime"); }
        }
        public bool AllTime
        {
            get
            {
                return allTime;
            }
            set
            {
                allTime = value;
                ReverseAllTime = !allTime;
                if (allTime)
                {
                    reservedStartDate = startDate.Date;
                    reservedEndDate = endDate.Date;
                    startDate = DateTime.MinValue;
                    SecondDate = DateTime.Now;
                }
                else
                {
                    startDate = reservedStartDate.Date;
                    SecondDate = reservedEndDate.Date;
                }
                OnPropertyChanged("AllTime");
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

        //Обработчик множественного выбора в ListBox
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

        //Генератор фильтров
        private void SetFilter<T>(ObservableCollection<T> list, IFilter filter, params string[] parameters) where T : IIdentifier
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

        //Свойства фильтруемых данных
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
                selectedSpares = value;
                SetFilter(SelectedSpares, filterSparesUsed, "SpareForModel", "Spare", "Rowid");
                OnPropertyChanged("SelectedSpares");
            }
        }

        public ObservableCollection<Service> SelectedServices
        {
            get { return (ObservableCollection<Service>)selectedServices; }
            set
            {
                selectedServices = value;
                SetFilter(SelectedServices, filterServicesDone, "ServiceForModel", "Service", "Rowid");
                OnPropertyChanged("SelectedServices");
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
                    if (dbContext is SQLiteContext)
                    {
                        SQLiteContext context = dbContext as SQLiteContext;
                        var parametersValues = context.ParameterValue.Where(pv => pv.RowidServiceLog == selectedServiceLog.Rowid).ToList();
                        var servicesDone = context.ServiceDone.Where(sd => sd.RowidServiceLog == selectedServiceLog.Rowid).ToList();
                        var sparesUsed = context.SpareUsed.Where(su => su.RowidServiceLog == selectedServiceLog.Rowid).ToList();
                        foreach (var item in parametersValues)
                        {
                            ParametersValues.Add(item);
                        }
                        foreach (var item in servicesDone)
                        {
                            ServicesDone.Add(item);
                        }
                        foreach (var item in sparesUsed)
                        {
                            SparesUsed.Add(item);
                        }
                    }                    
                }
                openEditServiceLogWindow?.RaiseCanExecuteChanged();
                openDialogWindow?.RaiseCanExecuteChanged();
                OnPropertyChanged("SelectedServiceLog");
            }
        }

        //Конструктор модели представления
        public ServiceLogVM()
        {
            string executable = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string path = (System.IO.Path.GetDirectoryName(executable));
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            parameter = System.Linq.Expressions.Expression.Parameter(typeof(ServiceLog), "s");
            parameterSD = System.Linq.Expressions.Expression.Parameter(typeof(ServiceDone), "sd");
            parameterSU = System.Linq.Expressions.Expression.Parameter(typeof(SpareUsed), "su");
            filterDate = new FilterDate(parameter);
            filterSearch = new FilterSearch(parameter);
            filterStatus = new FilterId(parameter);
            filterDept = new FilterId(parameter);
            filterRepairer = new FilterId(parameter);
            filterTypeModel = new FilterId(parameter);
            filterModel = new FilterId(parameter);
            filterSparesUsed = new FilterIdAnd(parameterSU);
            filterServicesDone = new FilterIdAnd(parameterSD);

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

            reservedStartDate = startDate.Date;
            reservedEndDate = endDate.Date;
            AllTime = false;
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

            DbContext tempDBContext = new SQLiteContext();
            SingletonDBContext sdbContext = SingletonDBContext.GetInstance(tempDBContext);
            dbContext = sdbContext.DBContext;
            if (dbContext is SQLiteContext)
            {
                SQLiteContext context = dbContext as SQLiteContext;
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
            OnFilterChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        //Обработчик фильтров
        public void OnFilterChanged()
        {
            System.Linq.Expressions.Expression result = null, temp;
            Delegate lambda = null, lambdaSU = null, lambdaSD = null;
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
            if (result != null)
            {
                lambda = System.Linq.Expressions.Expression.Lambda<Func<ServiceLog, bool>>(result, parameter).Compile();
            }      
            if(filterSparesUsed.GetFilter() != null)
                lambdaSU = System.Linq.Expressions.Expression.Lambda<Func<SpareUsed, bool>>(filterSparesUsed.GetFilter(), parameterSU).Compile();
            if (filterServicesDone.GetFilter() != null)
                lambdaSD = System.Linq.Expressions.Expression.Lambda<Func<ServiceDone, bool>>(filterServicesDone.GetFilter(), parameterSD).Compile();
            if (dbContext is SQLiteContext)
            {
                SQLiteContext context = dbContext as SQLiteContext;
                ServiceLogs = context.ServiceLog.Where((Func<ServiceLog, bool>)lambda).ToList().OrderBy(s => s.DateTime).ToList();
                if (lambdaSU != null)
                    ServiceLogs = ServiceLogs.Where(s => s.SparesUsed.Where((Func<SpareUsed, bool>)lambdaSU).Count() > 0).ToList();
                if (lambdaSD != null)
                    ServiceLogs = ServiceLogs.Where(s => s.ServicesDone.Where((Func<ServiceDone, bool>)lambdaSD).Count() > 0).ToList();
            }
            SelectedServiceLog = null;
            ParametersValues?.Clear();
            ServicesDone?.Clear();
            SparesUsed?.Clear();
        }
    }

    public class OpenWindowCommand : DelegateCommand
    {
        public OpenWindowCommand(Action<object> execute, ServiceLogVM main) : base(execute)
        {
        }

        public OpenWindowCommand(Action<object> execute, Func<object, bool> canExecute, ServiceLogVM main) : base(execute, canExecute)
        {
        }

        public override void Execute(object parameter)
        {
            base.Execute(parameter);
        }

        public override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter);
        }
    }

}
