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
using System.Diagnostics;

namespace EService.VVM.ViewModels
{
    public class ServiceLogVM : BaseVM
    {
        #region Поля
        //Поля модели представления
        private DbContext _dbContext;
        private string _search = String.Empty; //Поисковая строка

        private FilterSearch _filterSearch; //Фильтр поиска
        private FilterDate _filterDate; //Фильтр даты
        private FilterId _filterStatus, _filterRepairer, _filterDept, _filterTypeModel, _filterModel; //Фильтры по ID
        private FilterIdAnd _filterSparesUsed, _filterServicesDone; //Вложенные фильтры по ID
        private List<BL.IFilter> _filters; //Список всех фильтров
        ParameterExpression _parameter, _parameterSU, _parameterSD; //Параметры для формирования лямбды фильтрации     

        private DateTime _startDate; //Начальная дата
        private DateTime _endDate; //Конечная дата
        private DateTime _reservedStartDate; //Зарезервированная начальная дата
        private DateTime _reservedEndDate; //Зарезервированная конечная дата
        private bool _allTime; //Триггер для поиска за всё время
        private bool _reverseAllTime; //Блокировщик выбора даты

        private SView _selectedServiceLog; //Выбранная запись сервисного журнала
        //private ServiceLog selectedServiceLog; //Выбранная запись сервисного журнала
        private Status _selectedStatus; //Выбранный статус
        private Repairer _selectedRepairer; //Выбранный исполнитель ремонта
        private Dept _selectedDept; //Выбранный отдел
        private TypeModel _selectedTypeModel; //Выбранный тип устройства
        private Data.Entity.Model _selectedModel; //Выбранная модель
        private Spare _selectedSpare; //Выбранная запчасть
        private Service _selectedService; //Выбранный вид обслуживания

        private IList<SView> _serviceLogs; //Сервисный журнал

        private IList<Status> _selectedStatuses; //Список выбранных статусов
        private IList<Repairer> _selectedRepairers; //Список Выбранных исполнителей
        private IList<Dept> _selectedDepts; //Список выбранных отделов
        private IList<TypeModel> _selectedTypesModel; //Список выбранных типов устройств
        private IList<Data.Entity.Model> _selectedModels; //Список выбранных моделей устройств
        private IList<Spare> _selectedSpares; //Список выбранных запчастей
        private IList<Service> _selectedServices; //Список выбранных видов обслуживания

        private IDelegateCommand _openAddServiceLogWindow; //Команда открытия окна добавления записи в журнал
        private IDelegateCommand _openEditServiceLogWindow; //Команда открытия окна изменения записи в журнале
        private IDelegateCommand _refreshServiceLogWindow; //Команда обновления данных в окне
        private IDelegateCommand _openDialogWindow; //Команда открытия диалогового окна
        private IDelegateCommand _createReport; //Команда составления отчета
        #endregion

        #region Свойства
        //Команды для кнопок
        public IDelegateCommand AddServiceLogCommand
        {
            get
            {
                if (_openAddServiceLogWindow == null)
                {
                    _openAddServiceLogWindow = new OpenWindowCommand(ExecuteAddServiceLog, this);
                }
                return _openAddServiceLogWindow;
            }
        }
        public IDelegateCommand EditServiceLogCommand
        {
            get
            {
                if (_openEditServiceLogWindow == null)
                {
                    _openEditServiceLogWindow = new OpenWindowCommand(ExecuteEditServiceLog, CanExecuteEditServiceLog, this);
                }
                return _openEditServiceLogWindow;
            }
        }
        public IDelegateCommand RemoveServiceLogCommand
        {
            get
            {
                if (_openDialogWindow == null)
                {
                    _openDialogWindow = new OpenWindowCommand(OpenDialog, CanExecuteEditServiceLog, this);
                }
                return _openDialogWindow;
            }
        }
        public IDelegateCommand RefreshServiceLogCommand
        {
            get
            {
                if (_refreshServiceLogWindow == null)
                {
                    _refreshServiceLogWindow = new DelegateCommand(ExecuteRefreshServiceLog);
                }
                return _refreshServiceLogWindow;
            }
        }
        public IDelegateCommand CreateReportCommand
        {
            get
            {
                if (_createReport == null)
                {
                    _createReport = new DelegateCommand(ExecuteCreateReport);
                }
                return _createReport;
            }
        }

        //Свойства модели
        public Status SelectedStatus { get { return _selectedStatus; } set { _selectedStatus = value; OnPropertyChanged("SelectedStatus"); } }
        public Repairer SelectedRepairer { get { return _selectedRepairer; } set { _selectedRepairer = value; OnPropertyChanged("SelectedRepairer"); } }
        public Dept SelectedDept { get { return _selectedDept; } set { _selectedDept = value; OnPropertyChanged("SelectedDept"); } }
        public TypeModel SelectedTypeModel { get { return _selectedTypeModel; } set { _selectedTypeModel = value; OnPropertyChanged("SelectedTypeModel"); } }
        public Data.Entity.Model SelectedModel { get { return _selectedModel; } set { _selectedModel = value; OnPropertyChanged("SelectedModel"); } }
        public Spare SelectedSpare { get { return _selectedSpare; } set { _selectedSpare = value; OnPropertyChanged("SelectedSpare"); } }
        public Service SelectedService { get { return _selectedService; } set { _selectedService = value; OnPropertyChanged("SelectedService"); } }
        public String Search
        {
            get { return _search; }
            set
            {
                _search = value;
                _filterSearch.SetWhat(_search); // Задание поисковой строки
                _filterSearch.SetWhere("Device", "InventoryNumber"); // Задание пути для поиска
                _filterSearch.AddWhere(_filterSearch.Member); // Добавление пути в список путей
                _filterSearch.SetWhere("Device", "SerialNumber"); // Задание второго пути поиска
                _filterSearch.AddWhere(_filterSearch.Member); // Добавление второго пути в список путей
                _filterSearch.CreateFilter(); // Создание фильтра
                OnPropertyChanged("Search");
            }
        }
        public DateTime FirstDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                _filterDate.SetWhat(String.Format("{0}.{1}.{2}", _startDate.Day, _startDate.Month, _startDate.Year), String.Format("{0}.{1}.{2}", _endDate.Day, _endDate.Month, _endDate.Year));
                _filterDate.SetWhere("DateTime");
                _filterDate.CreateFilter();
            }
        }
        public DateTime SecondDate
        {
            get { return _endDate; }
            set
            {
                _endDate = value;
                _filterDate.SetWhat(String.Format("{0}.{1}.{2}", _startDate.Day, _startDate.Month, _startDate.Year), String.Format("{0}.{1}.{2}", _endDate.Day, _endDate.Month, _endDate.Year));
                _filterDate.SetWhere("DateTime");
                _filterDate.CreateFilter();
            }
        }
        public bool ReverseAllTime
        {
            get { return _reverseAllTime; }
            set { _reverseAllTime = value; OnPropertyChanged("ReverseAllTime"); }
        }
        public bool AllTime
        {
            get
            {
                return _allTime;
            }
            set
            {
                _allTime = value;
                ReverseAllTime = !_allTime;
                if (_allTime)
                {
                    _reservedStartDate = _startDate.Date;
                    _reservedEndDate = _endDate.Date;
                    _startDate = DateTime.MinValue;
                    SecondDate = DateTime.Now;
                }
                else
                {
                    _startDate = _reservedStartDate.Date;
                    SecondDate = _reservedEndDate.Date;
                }
                OnPropertyChanged("AllTime");
            }
        }
        public ObservableCollection<ParameterValue> ParametersValues { get; set; }
        public ObservableCollection<ServiceDone> ServicesDone { get; set; }
        public ObservableCollection<SpareUsed> SparesUsed { get; set; }
        public IList<SView> ServiceLogs { get { return _serviceLogs; } set { _serviceLogs = value; OnPropertyChanged("ServiceLogs"); } }
        public IList<Status> Statuses { get; set; }
        public IList<Repairer> Repairers { get; set; }
        public IList<Dept> Depts { get; set; }
        public IList<TypeModel> TypesModel { get; set; }
        public IList<Data.Entity.Model> Models { get; set; }
        public IList<Service> Services { get; set; }
        public IList<Spare> Spares { get; set; }

        //Свойства фильтруемых данных
        public ObservableCollection<Status> SelectedStatuses
        {
            get { return (ObservableCollection<Status>)_selectedStatuses; }
            set
            {
                _selectedStatuses = value;
                SetFilter(SelectedStatuses, _filterStatus, "Device", "Status", "Rowid");
                OnPropertyChanged("SelectedStatuses");
            }
        }

        public ObservableCollection<Repairer> SelectedRepairers
        {
            get { return (ObservableCollection<Repairer>)_selectedRepairers; }
            set
            {
                _selectedRepairers = value;
                SetFilter(SelectedRepairers, _filterRepairer, "Repairer", "Rowid");
                OnPropertyChanged("SelectedRepairers");
            }
        }

        public ObservableCollection<Dept> SelectedDepts
        {
            get { return (ObservableCollection<Dept>)_selectedDepts; }
            set
            {
                _selectedDepts = value;
                SetFilter(SelectedDepts, _filterDept, "Device", "Dept", "Rowid");
                OnPropertyChanged("SelectedDepts");
            }
        }

        public ObservableCollection<TypeModel> SelectedTypesModel
        {
            get { return (ObservableCollection<TypeModel>)_selectedTypesModel; }
            set
            {
                _selectedTypesModel = value;
                SetFilter(SelectedTypesModel, _filterTypeModel, "Device", "Model", "TypeModel", "Rowid");
                OnPropertyChanged("SelectedTypesModel");
            }
        }

        public ObservableCollection<Data.Entity.Model> SelectedModels
        {
            get { return (ObservableCollection<Data.Entity.Model>)_selectedModels; }
            set
            {
                _selectedModels = value;
                SetFilter(SelectedModels, _filterModel, "Device", "Model", "Rowid");
                OnPropertyChanged("SelectedModels");
            }
        }

        public ObservableCollection<Spare> SelectedSpares
        {
            get { return (ObservableCollection<Spare>)_selectedSpares; }
            set
            {
                _selectedSpares = value;
                SetFilter(SelectedSpares, _filterSparesUsed, "SpareForModel", "Spare", "Rowid");
                OnPropertyChanged("SelectedSpares");
            }
        }

        public ObservableCollection<Service> SelectedServices
        {
            get { return (ObservableCollection<Service>)_selectedServices; }
            set
            {
                _selectedServices = value;
                SetFilter(SelectedServices, _filterServicesDone, "ServiceForModel", "Service", "Rowid");
                OnPropertyChanged("SelectedServices");
            }
        }

        public SView SelectedServiceLog
        {
            get { return _selectedServiceLog; }
            set
            {
                _selectedServiceLog = value;
                if (_selectedServiceLog != null)
                {
                    ParametersValues.Clear();
                    ServicesDone.Clear();
                    SparesUsed.Clear();
                    if (_dbContext is SQLiteContext)
                    {
                        SQLiteContext context = _dbContext as SQLiteContext;
                        context.ParameterValue.Where(pv => pv.RowidServiceLog == _selectedServiceLog.ServiceLog.Rowid).ToList().ForEach(item => ParametersValues.Add(item));
                        context.ServiceDone.Where(sd => sd.RowidServiceLog == _selectedServiceLog.ServiceLog.Rowid).ToList().ForEach(item => ServicesDone.Add(item));
                        context.SpareUsed.Where(su => su.RowidServiceLog == _selectedServiceLog.ServiceLog.Rowid).ToList().ForEach(item => SparesUsed.Add(item));
                    }
                }
                _openEditServiceLogWindow?.RaiseCanExecuteChanged();
                _openDialogWindow?.RaiseCanExecuteChanged();
                OnPropertyChanged("SelectedServiceLog");
            }
        }

        #endregion

        #region Конструкторы

        //Конструктор модели представления
        public ServiceLogVM()
        {
            InitializeFilters();
            InitializeData();
        }

        #endregion        

        #region Методы
        public override void Refresh()
        {
            OnFilterChanged();
        }

        private void InitializeData()
        {
            _reservedStartDate = _startDate.Date;
            _reservedEndDate = _endDate.Date;
            AllTime = false;
            FirstDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            SecondDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            Spares = new ObservableCollection<Spare>();
            Services = new ObservableCollection<Service>();
            Models = new ObservableCollection<Data.Entity.Model>();
            TypesModel = new ObservableCollection<TypeModel>();
            Depts = new ObservableCollection<Dept>();
            Repairers = new ObservableCollection<Repairer>();
            Statuses = new ObservableCollection<Status>();
            ServiceLogs = new ObservableCollection<SView>();
            ParametersValues = new ObservableCollection<ParameterValue>();
            ServicesDone = new ObservableCollection<ServiceDone>();
            SparesUsed = new ObservableCollection<SpareUsed>();
            SelectedStatuses = new ObservableCollection<Status>();
            SelectedRepairers = new ObservableCollection<Repairer>();
            SelectedDepts = new ObservableCollection<Dept>();
            SelectedTypesModel = new ObservableCollection<TypeModel>();
            SelectedModels = new ObservableCollection<Data.Entity.Model>();
            SelectedSpares = new ObservableCollection<Spare>();
            SelectedServices = new ObservableCollection<Service>();

            _dbContext = SingletonDBContext.GetInstance(new SQLiteContext()).DBContext;
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
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
        private void ServiceLogsListCreator(IList<ServiceLog> list)
        {
            ServiceLogs.Clear();
            list.ToList().ForEach(item => ServiceLogs.Add(new SView(item, ServiceLogs.Count() + 1)));
        }

        //Обработчики для команд
        private void ExecuteAddServiceLog(object parameter)
        {
            var displayRootRegistry = (System.Windows.Application.Current as App).displayRootRegistry;
            var addServiceLogVM = new AddServiceLogVM();
            displayRootRegistry.ShowPresentation(addServiceLogVM);
        }
        private async void ExecuteEditServiceLog(object parameter)
        {
            var displayRootRegistry = (System.Windows.Application.Current as App).displayRootRegistry;
            var editServiceLogVM = new EditServiceLogVM(_selectedServiceLog.ServiceLog.Rowid);
            await displayRootRegistry.ShowModalPresentation(editServiceLogVM);
        }
        private void ExecuteRemoveServiceLog(object parameter)
        {
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Configuration.LazyLoadingEnabled = false;
                context.ServiceLog.Remove(_selectedServiceLog.ServiceLog);
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
            var message = String.Format("Вы действительно хотите удалить запись ремонта {0} I/N - \"{1}\" | S/N - \"{2}\"?", _selectedServiceLog.ServiceLog.Device.Model.TypeModel.ShortName, _selectedServiceLog.ServiceLog.Device.InventoryNumber, _selectedServiceLog.ServiceLog.Device.SerialNumber);
            var displayRootRegistry = (System.Windows.Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM("Удаление записи", message, ExecuteRemoveServiceLog);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        private async void ExecuteCreateReport(object parameter)
        {
            var displayRootRegistry = (System.Windows.Application.Current as App).displayRootRegistry;
            var createReport = new ReportVM(_serviceLogs);
            await displayRootRegistry.ShowModalPresentation(createReport);
        }
        private bool CanExecuteEditServiceLog(object parameter)
        {
            if (_selectedServiceLog != null)
                return true;
            return false;
        }

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

                temp = ItemsBuilder.SelectItem(value, SelectedTypesModel, typeof(TypeModel), SelectedTypeModel);
                if (temp != null) SelectedTypesModel = (ObservableCollection<TypeModel>)temp;

                temp = ItemsBuilder.SelectItem(value, SelectedModels, typeof(Data.Entity.Model), SelectedModel);
                if (temp != null) SelectedModels = (ObservableCollection<Data.Entity.Model>)temp;

                temp = ItemsBuilder.SelectItem(value, SelectedSpares, typeof(Spare), SelectedSpare);
                if (temp != null) SelectedSpares = (ObservableCollection<Spare>)temp;

                temp = ItemsBuilder.SelectItem(value, SelectedServices, typeof(Service), SelectedService);
                if (temp != null) SelectedServices = (ObservableCollection<Service>)temp;

                OnFilterChanged();
            }
        }

        //Генератор фильтров
        private void SetFilter<T>(ObservableCollection<T> list, BL.IFilter filter, params string[] parameters) where T : IIdentifier
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

        //Обработчик фильтров
        public void OnFilterChanged()
        {
            System.Linq.Expressions.Expression result = null, temp;
            Delegate lambda = null, lambdaSU = null, lambdaSD = null;
            foreach (var item in _filters)
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
                lambda = System.Linq.Expressions.Expression.Lambda<Func<ServiceLog, bool>>(result, _parameter).Compile();
            }
            if (_filterSparesUsed.GetFilter() != null)
                lambdaSU = System.Linq.Expressions.Expression.Lambda<Func<SpareUsed, bool>>(_filterSparesUsed.GetFilter(), _parameterSU).Compile();
            if (_filterServicesDone.GetFilter() != null)
                lambdaSD = System.Linq.Expressions.Expression.Lambda<Func<ServiceDone, bool>>(_filterServicesDone.GetFilter(), _parameterSD).Compile();
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                var tempServiceLogs = context.ServiceLog.Where((Func<ServiceLog, bool>)lambda).ToList().OrderBy(s => s.DateTime).ToList();
                if (lambdaSU != null)
                    tempServiceLogs = tempServiceLogs.Where(s => s.SparesUsed.Where((Func<SpareUsed, bool>)lambdaSU).Count() > 0).ToList();
                if (lambdaSD != null)
                    tempServiceLogs = tempServiceLogs.Where(s => s.ServicesDone.Where((Func<ServiceDone, bool>)lambdaSD).Count() > 0).ToList();
                ServiceLogsListCreator(tempServiceLogs);
            }
            SelectedServiceLog = null;
            ParametersValues?.Clear();
            ServicesDone?.Clear();
            SparesUsed?.Clear();
        }

        private void InitializeFilters()
        {
            _parameter = System.Linq.Expressions.Expression.Parameter(typeof(ServiceLog), "s");
            _parameterSD = System.Linq.Expressions.Expression.Parameter(typeof(ServiceDone), "sd");
            _parameterSU = System.Linq.Expressions.Expression.Parameter(typeof(SpareUsed), "su");
            _filterDate = new FilterDate(_parameter);
            _filterSearch = new FilterSearch(_parameter);
            _filterStatus = new FilterId(_parameter);
            _filterDept = new FilterId(_parameter);
            _filterRepairer = new FilterId(_parameter);
            _filterTypeModel = new FilterId(_parameter);
            _filterModel = new FilterId(_parameter);
            _filterSparesUsed = new FilterIdAnd(_parameterSU);
            _filterServicesDone = new FilterIdAnd(_parameterSD);

            _filters = new List<BL.IFilter>();

            _filters.Add(_filterDate);
            _filters.Add(_filterSearch);
            _filters.Add(_filterStatus);
            _filters.Add(_filterRepairer);
            _filters.Add(_filterDept);
            _filters.Add(_filterTypeModel);
            _filters.Add(_filterModel);

            _filterDate.FilterCreated += OnFilterChanged;
            _filterSearch.FilterCreated += OnFilterChanged;
        }
        #endregion        
    }

}
