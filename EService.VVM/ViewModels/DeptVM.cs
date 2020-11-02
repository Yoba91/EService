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
using System.Windows;

namespace EService.VVM.ViewModels
{
    class DeptVM : BaseVM
    {
        #region Поля
        private DbContext _dbContext;
        private string _search = String.Empty; //Поисковая строка

        private FilterSearch _filterSearch; //Фильтр поиска
        private FilterId _filterDevice; //Фильтры по ID
        private List<IFilter> _filters; //Список всех фильтров
        private ParameterExpression _parameter, _parameterDevice; //Параметр для формирования лямбды фильтрации     

        private DView _selectedDept; //Выбранный отдел
        private Model _selectedModel; //Выбранная модель
        private TypeModel _selectedTypeModel; //Выбранный тип модели
        private Status _selectedStatus; //Выбранный статус

        private IList<Model> _selectedModels; //Список выбранных моделей устройств
        private IList<TypeModel> _selectedTypesModel; //Список выбранных моделей устройств
        private IList<Status> _selectedStatuses; //Список выбранных моделей устройств

        private IList<Model> _models; //Список моделей устройств
        private IList<TypeModel> _typesModel; //Список типов моделей устройств
        private IList<Status> _statuses; //Список статусов устройств

        private IDelegateCommand _openAddDeptWindow; //Команда открытия окна добавления отдела
        private IDelegateCommand _openEditDeptWindow; //Команда открытия окна изменения отдела
        private IDelegateCommand _refreshDeptWindow; //Команда обновления данных в окне
        private IDelegateCommand _openDialogWindow; //Команда открытия диалогового окна
        #endregion

        #region Свойства
        //Команды для кнопок
        public IDelegateCommand AddDeptCommand
        {
            get
            {
                if (_openAddDeptWindow == null)
                {
                    _openAddDeptWindow = new OpenWindowCommand(ExecuteAdd, this);
                }
                return _openAddDeptWindow;
            }
        }
        public IDelegateCommand EditDeptCommand
        {
            get
            {
                if (_openEditDeptWindow == null)
                {
                    _openEditDeptWindow = new OpenWindowCommand(ExecuteEdit, CanExecuteEdit, this);
                }
                return _openEditDeptWindow;
            }
        }
        public IDelegateCommand RemoveDeptCommand
        {
            get
            {
                if (_openDialogWindow == null)
                {
                    _openDialogWindow = new OpenWindowCommand(OpenDialog, CanExecuteEdit, this);
                }
                return _openDialogWindow;
            }
        }
        public IDelegateCommand RefreshDeptCommand
        {
            get
            {
                if (_refreshDeptWindow == null)
                {
                    _refreshDeptWindow = new DelegateCommand(ExecuteRefresh);
                }
                return _refreshDeptWindow;
            }
        }
        //Свойства модели
        public DView SelectedDept { get { return _selectedDept; } set { _selectedDept = value; OnPropertyChanged("SelectedService"); _openEditDeptWindow?.RaiseCanExecuteChanged(); _openDialogWindow?.RaiseCanExecuteChanged(); } }
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
                _filterSearch.SetWhere("Name"); // Задание пути для поиска
                _filterSearch.AddWhere(_filterSearch.Member); // Добавление пути в список путей
                _filterSearch.SetWhere("Code"); // Задание пути для поиска
                _filterSearch.AddWhere(_filterSearch.Member); // Добавление пути в список путей
                _filterSearch.SetWhere("Description"); // Задание пути для поиска
                _filterSearch.AddWhere(_filterSearch.Member); // Добавление пути в список путей
                _filterSearch.CreateFilter(); // Создание фильтра
                OnPropertyChanged("Search");
            }
        }
        public IList<DView> Depts { get; set; }
        public IList<Model> Models { get { return _models; } set { _models = value; OnPropertyChanged("Models"); } }
        public IList<TypeModel> TypesModel { get { return _typesModel; } set { _typesModel = value; OnPropertyChanged("TypesModel"); } }
        public IList<Status> Statuses { get { return _statuses; } set { _statuses = value; OnPropertyChanged("Statuses"); } }
        public ObservableCollection<Model> SelectedModels
        {
            get { return (ObservableCollection<Model>)_selectedModels; }
            set
            {
                _selectedModels = value;
                SetFilter(SelectedModels, _filterDevice, "Model", "Rowid");
                OnPropertyChanged("SelectedModels");
            }
        }
        public ObservableCollection<TypeModel> SelectedTypesModel
        {
            get { return (ObservableCollection<TypeModel>)_selectedTypesModel; }
            set
            {
                _selectedTypesModel = value;
                SetFilter(SelectedTypesModel, _filterDevice, "Model", "TypeModel", "Rowid");
                OnPropertyChanged("SelectedTypesModel");
            }
        }
        public ObservableCollection<Status> SelectedStatuses
        {
            get { return (ObservableCollection<Status>)_selectedStatuses; }
            set
            {
                _selectedStatuses = value;
                SetFilter(SelectedStatuses, _filterDevice, "Status", "Rowid");
                OnPropertyChanged("SelectedStatuses");
            }
        }
        #endregion

        #region Конструктор
        public DeptVM()
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
            Depts = new ObservableCollection<DView>();
            SelectedModels = new ObservableCollection<Model>();
            SelectedTypesModel = new ObservableCollection<TypeModel>();
            SelectedStatuses = new ObservableCollection<Status>();
            _dbContext = SingletonDBContext.GetInstance(new SQLiteContext()).DBContext;
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Model.Load();
                Models = context.Model.Local.ToBindingList();
                context.TypeModel.Load();
                TypesModel = context.TypeModel.Local.ToBindingList();
                context.Status.Load();
                Statuses = context.Status.Local.ToBindingList();
                context.Dept.Load();
                var deptsList = context.Dept.Local.ToBindingList();
                DeptsListCreator(deptsList, null);
            }
        }
        private void InitializeFilters()
        {
            _parameter = System.Linq.Expressions.Expression.Parameter(typeof(Dept), "s");
            _parameterDevice = System.Linq.Expressions.Expression.Parameter(typeof(Device), "s");
            _filterSearch = new FilterSearch(_parameter);
            _filterDevice = new FilterId(_parameterDevice);

            _filters = new List<IFilter>();

            _filters.Add(_filterSearch);

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
        private void DeptsListCreator(IList<Dept> list, Delegate lambdaDevice)
        {
            Depts.Clear();
            foreach (var item in list)
            {
                var devices = item.Devices;
                var devicesCount = devices.Count();
                if (lambdaDevice != null)
                    devicesCount = devices.Where((Func<Device, bool>)lambdaDevice).Count();
                Depts.Add(new DView(item, devicesCount, Depts.Count() + 1));
            }
        }

        public System.Collections.IList SelectedItems
        {
            set
            {
                System.Collections.IList temp = null;

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
            IList<Dept> tempDepts = new ObservableCollection<Dept>();
            System.Linq.Expressions.Expression result = null, temp;
            Delegate lambda = null, lambdaD = null;
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
                lambda = System.Linq.Expressions.Expression.Lambda<Func<Dept, bool>>(result, _parameter).Compile();
            }
            if (_filterDevice.GetFilter() != null)
                lambdaD = System.Linq.Expressions.Expression.Lambda<Func<Device, bool>>(_filterDevice.GetFilter(), _parameterDevice).Compile();
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                tempDepts = context.Dept.ToList();
                if (lambda != null)
                    tempDepts = context.Dept.Where((Func<Dept, bool>)lambda).ToList();
                if (lambdaD != null)
                    tempDepts = tempDepts.Where(tm => tm.Devices.Where((Func<Device, bool>)lambdaD).Count() > 0).ToList();
                DeptsListCreator(tempDepts, lambdaD);
            }
            SelectedDept = null;
        }
        //Обработчики для команд
        private void ExecuteAdd(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var addDeptVM = new AddDeptVM();
            displayRootRegistry.ShowPresentation(addDeptVM);
        }
        private async void ExecuteEdit(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var editDeptVM = new EditDeptVM(_selectedDept.Dept.Rowid);
            await displayRootRegistry.ShowModalPresentation(editDeptVM);
        }
        private void ExecuteRemove(object parameter)
        {
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Configuration.LazyLoadingEnabled = false;
                context.Dept.Remove(_selectedDept.Dept);
                context.SaveChanges();
                SelectedDept = null;
                OnFilterChanged();
            }
        }
        private void ExecuteRefresh(object parameter)
        {
            OnFilterChanged();
        }
        private async void OpenDialog(object parameter)
        {
            var message = String.Format("Вы действительно хотите удалить отдел {0}({1}), и все связанные с ним устройства и записи?", _selectedDept.Dept.Name, _selectedDept.Dept.Code);
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM("Удаление отела", message, ExecuteRemove);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        private bool CanExecuteEdit(object parameter)
        {
            if (_selectedDept != null)
                return true;
            return false;
        }
        #endregion        

        public class DView
        {
            public Dept Dept { get; private set; }
            public int DevicesCount { get; set; }
            public int Index { get; set; }

            public DView(Dept dept, int devicesCount, int index)
            {
                Dept = dept;
                DevicesCount = devicesCount;
                Index = index;
            }
        }
    }
}
