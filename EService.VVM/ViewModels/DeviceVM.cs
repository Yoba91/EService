using EService.BL;
using EService.Data.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EService.VVM.ViewModels
{
    class DeviceVM : BaseVM
    {
        #region Поля
        private DbContext _dbContext;
        private string _search = String.Empty; //Поисковая строка

        private FilterSearch _filterSearch; //Фильтр поиска
        private FilterId _filterDept, _filterModel, _filterTypeModel, _filterStatus; //Фильтры по ID
        private List<IFilter> _filters; //Список всех фильтров
        private ParameterExpression _parameter; //Параметр для формирования лямбды фильтрации     

        private DView _selectedDevice; //Выбранное устройство
        private Dept _selectedDept; //Выбранный отдел        
        private Model _selectedModel; //Выбранная модель
        private TypeModel _selectedTypeModel; //Выбранный тип модели
        private Status _selectedStatus; //Выбранный статус

        private IList<Dept> _selectedDepts; //Список выбранных отделов
        private IList<Model> _selectedModels; //Список выбранных моделей устройств
        private IList<TypeModel> _selectedTypesModel; //Список выбранных типов модели
        private IList<Status> _selectedStatuses; //Список выбранных статусов

        private IDelegateCommand _openAddTypeModelWindow; //Команда открытия окна добавления записи в журнал
        private IDelegateCommand _openEditTypeModelWindow; //Команда открытия окна изменения записи в журнале
        private IDelegateCommand _refreshTypeModelWindow; //Команда обновления данных в окне
        private IDelegateCommand _openDialogWindow; //Команда открытия диалогового окна
        #endregion

        #region Свойства
        public DView SelectedDevice { get { return _selectedDevice; } set { _selectedDevice = value; OnPropertyChanged("SelectedDevice"); } }
        public Dept SelectedDept { get { return _selectedDept; } set { _selectedDept = value; OnPropertyChanged("SelectedDept"); } }
        public Model SelectedModel { get { return _selectedModel; } set { _selectedModel = value; OnPropertyChanged("SelectedModel"); } }
        public TypeModel SelectedTypeModel { get { return _selectedTypeModel; } set { _selectedTypeModel = value; OnPropertyChanged("SelectedTypeModel"); } }
        public Status SelectedStatus { get { return _selectedStatus; } set { _selectedStatus = value; OnPropertyChanged("SelectedStatus"); } }
        public String Search
        {
            get { return _search; }
            set
            {
                _search = value;
                _filterSearch.SetWhat(_search); // Задание поисковой строки
                _filterSearch.SetWhere("InventoryNumber"); // Задание пути для поиска
                _filterSearch.AddWhere(_filterSearch.Member); // Добавление пути в список путей
                _filterSearch.SetWhere("SerialNumber"); // Задание второго пути поиска
                _filterSearch.AddWhere(_filterSearch.Member); // Добавление второго пути в список путей
                _filterSearch.CreateFilter(); // Создание фильтра
                OnPropertyChanged("Search");
            }
        }
        public IList<DView> Devices { get; set; }
        public IList<Dept> Depts { get; set; }
        public IList<Model> Models { get; set; }
        public IList<TypeModel> TypesModel { get; set; }
        public IList<Status> Statuses { get; set; }
        public ObservableCollection<Dept> SelectedDepts
        {
            get { return (ObservableCollection<Dept>)_selectedDepts; }
            set
            {
                _selectedDepts = value;
                SetFilter(SelectedDepts, _filterDept, "Dept", "Rowid");
                OnPropertyChanged("SelectedDepts");
            }
        }
        public ObservableCollection<Model> SelectedModels
        {
            get { return (ObservableCollection<Model>)_selectedModels; }
            set
            {
                _selectedModels = value;
                SetFilter(SelectedModels, _filterModel, "Model", "Rowid");
                OnPropertyChanged("SelectedModels");
            }
        }
        public ObservableCollection<TypeModel> SelectedTypesModel
        {
            get { return (ObservableCollection<TypeModel>)_selectedTypesModel; }
            set
            {
                _selectedTypesModel = value;
                SetFilter(SelectedTypesModel, _filterTypeModel, "Model", "TypeModel", "Rowid");
                OnPropertyChanged("SelectedTypesModel");
            }
        }
        public ObservableCollection<Status> SelectedStatuses
        {
            get { return (ObservableCollection<Status>)_selectedStatuses; }
            set
            {
                _selectedStatuses = value;
                SetFilter(SelectedStatuses, _filterStatus,"Status", "Rowid");
                OnPropertyChanged("SelectedStatuses");
            }
        }
        #endregion

        #region Конструктор
        public DeviceVM()
        {
            InitializeFilters();
            Devices = new ObservableCollection<DView>();
            SelectedDepts = new ObservableCollection<Dept>();
            SelectedModels = new ObservableCollection<Model>();
            SelectedTypesModel = new ObservableCollection<TypeModel>();
            SelectedStatuses = new ObservableCollection<Status>();
            _dbContext = SingletonDBContext.GetInstance(new SQLiteContext()).DBContext;
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Dept.Load();
                Depts = context.Dept.Local.ToBindingList();
                context.Model.Load();
                Models = context.Model.Local.ToBindingList();
                context.Model.Load();
                TypesModel = context.TypeModel.Local.ToBindingList();
                context.Status.Load();
                Statuses = context.Status.Local.ToBindingList();
                context.Device.Load();
                var devicesList = context.Device.Local.ToBindingList();
                DevicesListCreator(devicesList);
            }

        }
        #endregion

        #region Методы
        private void InitializeFilters()
        {
            _parameter = System.Linq.Expressions.Expression.Parameter(typeof(Device), "s");
            _filterSearch = new FilterSearch(_parameter);
            _filterDept = new FilterId(_parameter);
            _filterModel = new FilterId(_parameter);
            _filterTypeModel = new FilterId(_parameter);
            _filterStatus = new FilterId(_parameter);

            _filters = new List<IFilter>();

            _filters.Add(_filterSearch);
            _filters.Add(_filterDept);
            _filters.Add(_filterModel);
            _filters.Add(_filterTypeModel);
            _filters.Add(_filterStatus);

            _filterSearch.FilterCreated += OnFilterChanged;
        }

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

        private void DevicesListCreator(IList<Device> list)
        {
            Devices.Clear();
            foreach (var item in list)
            {
                int serviceLogCount = item.ServiceLogs.Count();
                Devices.Add(new DView(item, serviceLogCount, Devices.Count() + 1));                
            }
        }

        public System.Collections.IList SelectedItems
        {
            set
            {
                System.Collections.IList temp = null;

                temp = ItemsBuilder.SelectItem(value, SelectedDepts, typeof(Dept), SelectedDept);
                if (temp != null) SelectedDepts = (ObservableCollection<Dept>)temp;

                temp = ItemsBuilder.SelectItem(value, SelectedModels, typeof(Model), SelectedModel);
                if (temp != null) SelectedModels = (ObservableCollection<Model>)temp;

                temp = ItemsBuilder.SelectItem(value, SelectedTypesModel, typeof(TypeModel), SelectedTypeModel);
                if (temp != null) SelectedTypesModel = (ObservableCollection<TypeModel>)temp;

                temp = ItemsBuilder.SelectItem(value, SelectedStatuses, typeof(Status), SelectedStatus);
                if (temp != null) SelectedStatuses = (ObservableCollection<Status>)temp;

                OnFilterChanged();
            }
        }

        public void OnFilterChanged()
        {
            IList<Device> tempDevices = new ObservableCollection<Device>();
            System.Linq.Expressions.Expression result = null, temp;
            Delegate lambda = null;
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
                lambda = System.Linq.Expressions.Expression.Lambda<Func<Device, bool>>(result, _parameter).Compile();
            }
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                tempDevices = context.Device.ToList();
                if (lambda != null)
                    tempDevices = context.Device.Where((Func<Device, bool>)lambda).ToList();                
                DevicesListCreator(tempDevices);
            }
            SelectedDevice = null;
        }
        #endregion

        public class DView
        {
            public Device Device { get; private set; }
            public int ServiceLogCount { get; private set; }
            public int Index { get; set; } 

            public DView(Device device, int serviceLogCount, int index)
            {
                Device = device;
                ServiceLogCount = serviceLogCount;
                Index = index;
            }
        }
    }
}
