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
    class StatusVM : BaseVM
    {
        #region Поля
        private DbContext _dbContext;
        private string _search = String.Empty; //Поисковая строка

        private FilterSearch _filterSearch; //Фильтр поиска
        private FilterId _filterDevice; //Фильтры по ID
        private List<IFilter> _filters; //Список всех фильтров
        private ParameterExpression _parameter, _parameterDevice; //Параметр для формирования лямбды фильтрации     

        private SView _selectedStatus; //Выбранный статус
        private Model _selectedModel; //Выбранная модель
        private TypeModel _selectedTypeModel; //Выбранный тип модели

        private IList<Model> _selectedModels; //Список выбранных моделей устройств
        private IList<TypeModel> _selectedTypesModel; //Список выбранных моделей устройств

        private IList<Model> _models; //Список моделей устройств
        private IList<TypeModel> _typesModel; //Список типов моделей устройств

        private IDelegateCommand _openAddStatusWindow; //Команда открытия окна добавления статуса
        private IDelegateCommand _openEditStatusWindow; //Команда открытия окна изменения статуса
        private IDelegateCommand _refreshStatusWindow; //Команда обновления данных в окне
        private IDelegateCommand _openDialogWindow; //Команда открытия диалогового окна
        #endregion

        #region Свойства
        //Команды для кнопок
        public IDelegateCommand AddStatusCommand
        {
            get
            {
                if (_openAddStatusWindow == null)
                {
                    _openAddStatusWindow = new OpenWindowCommand(ExecuteAddStatus, this);
                }
                return _openAddStatusWindow;
            }
        }
        public IDelegateCommand EditStatusCommand
        {
            get
            {
                if (_openEditStatusWindow == null)
                {
                    _openEditStatusWindow = new OpenWindowCommand(ExecuteEditStatus, CanExecuteEditStatus, this);
                }
                return _openEditStatusWindow;
            }
        }
        public IDelegateCommand RemoveStatusCommand
        {
            get
            {
                if (_openDialogWindow == null)
                {
                    _openDialogWindow = new OpenWindowCommand(OpenDialog, CanExecuteEditStatus, this);
                }
                return _openDialogWindow;
            }
        }
        public IDelegateCommand RefreshStatusCommand
        {
            get
            {
                if (_refreshStatusWindow == null)
                {
                    _refreshStatusWindow = new DelegateCommand(ExecuteRefreshStatus);
                }
                return _refreshStatusWindow;
            }
        }
        //Свойства модели
        public SView SelectedStatus { get { return _selectedStatus; } set { _selectedStatus = value; OnPropertyChanged("SelectedStatus"); _openEditStatusWindow?.RaiseCanExecuteChanged(); _openDialogWindow?.RaiseCanExecuteChanged(); } }
        public Model SelectedModel { get { return _selectedModel; } set { _selectedModel = value; OnPropertyChanged("SelectedModel"); } }
        public TypeModel SelectedTypeModel { get { return _selectedTypeModel; } set { _selectedTypeModel = value; OnPropertyChanged("SelectedTypeModel"); } }
        public String Search
        {
            get { return _search; }
            set
            {
                _search = value;
                _filterSearch.SetWhat(_search); // Задание поисковой строки
                _filterSearch.SetWhere("Name"); // Задание пути для поиска
                _filterSearch.AddWhere(_filterSearch.Member); // Добавление пути в список путей
                _filterSearch.CreateFilter(); // Создание фильтра
                OnPropertyChanged("Search");
            }
        }
        public IList<SView> Statuses { get; set; }
        public IList<Model> Models { get { return _models; } set { _models = value; OnPropertyChanged("Models"); } }
        public IList<TypeModel> TypesModel { get { return _typesModel; } set { _typesModel = value; OnPropertyChanged("TypesModel"); } }
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
        #endregion

        #region Конструктор
        public StatusVM()
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
            Statuses = new ObservableCollection<SView>();
            SelectedModels = new ObservableCollection<Model>();
            SelectedTypesModel = new ObservableCollection<TypeModel>();
            _dbContext = SingletonDBContext.GetInstance(new SQLiteContext()).DBContext;
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Model.Load();
                Models = context.Model.Local.ToBindingList();
                context.TypeModel.Load();
                TypesModel = context.TypeModel.Local.ToBindingList();
                context.Status.Load();
                var statusesList = context.Status.Local.ToBindingList();
                StatusesListCreator(statusesList, null);
            }
        }
        private void InitializeFilters()
        {
            _parameter = System.Linq.Expressions.Expression.Parameter(typeof(Status), "s");
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
        private void StatusesListCreator(IList<Status> list, Delegate lambdaDevice)
        {
            Statuses.Clear();
            foreach (var item in list)
            {
                var devices = item.Devices;
                var devicesCount = devices.Count();
                if (lambdaDevice != null)
                    devicesCount = devices.Where((Func<Device, bool>)lambdaDevice).Count();
                Statuses.Add(new SView(item, devicesCount, Statuses.Count() + 1));
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

                OnFilterChanged();
            }
        }

        public void OnFilterChanged()
        {
            IList<Status> tempStatuses = new ObservableCollection<Status>();
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
                lambda = System.Linq.Expressions.Expression.Lambda<Func<Status, bool>>(result, _parameter).Compile();
            }
            if (_filterDevice.GetFilter() != null)
                lambdaD = System.Linq.Expressions.Expression.Lambda<Func<Device, bool>>(_filterDevice.GetFilter(), _parameterDevice).Compile();
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                tempStatuses = context.Status.ToList();
                if (lambda != null)
                    tempStatuses = context.Status.Where((Func<Status, bool>)lambda).ToList();
                if (lambdaD != null)
                    tempStatuses = tempStatuses.Where(tm => tm.Devices.Where((Func<Device, bool>)lambdaD).Count() > 0).ToList();
                StatusesListCreator(tempStatuses, lambdaD);
            }
            SelectedStatus = null;
        }
        //Обработчики для команд
        private void ExecuteAddStatus(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var addStatusVM = new AddStatusVM();
            displayRootRegistry.ShowPresentation(addStatusVM);
        }
        private async void ExecuteEditStatus(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var editStatusVM = new EditStatusVM(_selectedStatus.Status.Rowid);
            await displayRootRegistry.ShowModalPresentation(editStatusVM);
        }
        private void ExecuteRemoveStatus(object parameter)
        {
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Configuration.LazyLoadingEnabled = false;
                context.Status.Remove(_selectedStatus.Status);
                context.SaveChanges();
                SelectedStatus = null;
                OnFilterChanged();
            }
        }
        private void ExecuteRefreshStatus(object parameter)
        {
            OnFilterChanged();
        }
        private async void OpenDialog(object parameter)
        {
            var message = String.Format("Вы действительно хотите удалить статус {0}, и все связанные с ним устройства и записи?", _selectedStatus.Status.Name);
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM("Удаление статуса", message, ExecuteRemoveStatus);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        private bool CanExecuteEditStatus(object parameter)
        {
            if (_selectedStatus != null)
                return true;
            return false;
        }
        #endregion

        public class SView
        {
            public Status Status { get; private set; }
            public int DevicesCount { get; set; }
            public int Index { get; set; }

            public SView(Status status, int devicesCount, int index)
            {
                Status = status;
                DevicesCount = devicesCount;
                Index = index;
            }
        }
    }
}
