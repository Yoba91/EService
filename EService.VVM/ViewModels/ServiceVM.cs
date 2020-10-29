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
    class ServiceVM : BaseVM
    {
        #region Поля
        private DbContext _dbContext;
        private string _search = String.Empty; //Поисковая строка

        private FilterSearch _filterSearch; //Фильтр поиска
        private List<IFilter> _filters; //Список всех фильтров
        private ParameterExpression _parameter; //Параметр для формирования лямбды фильтрации     

        private SView _selectedService; //Выбранный сервис
        private Model _selectedModel; //Выбранная модель
        private ServiceForModel _selectedServiceForModel; //Выбранный сервис привязанный к модели

        private IList<Model> _selectedModels; //Список выбранных моделей устройств
        private IList<ServiceForModel> _selectedServiceForModels; //Список выбранных сервисов привязанных к модели

        private IList<Model> _models; //Список моделей устройств
        private IList<ServiceForModel> _serviceForModels; //Список сервисов привязанных к модели

        private IDelegateCommand _openAddServiceWindow; //Команда открытия окна добавления записи в журнал
        private IDelegateCommand _openEditServiceWindow; //Команда открытия окна изменения записи в журнале
        private IDelegateCommand _refreshServiceWindow; //Команда обновления данных в окне
        private IDelegateCommand _openDialogWindow; //Команда открытия диалогового окна
        #endregion

        #region Свойства
        //Команды для кнопок
        public IDelegateCommand AddServiceCommand
        {
            get
            {
                if (_openAddServiceWindow == null)
                {
                    _openAddServiceWindow = new OpenWindowCommand(ExecuteAdd, this);
                }
                return _openAddServiceWindow;
            }
        }
        public IDelegateCommand EditServiceCommand
        {
            get
            {
                if (_openEditServiceWindow == null)
                {
                    _openEditServiceWindow = new OpenWindowCommand(ExecuteEdit, CanExecuteEdit, this);
                }
                return _openEditServiceWindow;
            }
        }
        public IDelegateCommand RemoveServiceCommand
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
        public IDelegateCommand RefreshServiceCommand
        {
            get
            {
                if (_refreshServiceWindow == null)
                {
                    _refreshServiceWindow = new DelegateCommand(ExecuteRefresh);
                }
                return _refreshServiceWindow;
            }
        }
        //Свойства модели
        public SView SelectedService
        {
            get
            {
                return _selectedService;
            }
            set
            {
                _selectedService = value;
                if (_dbContext is SQLiteContext)
                {
                    SQLiteContext context = _dbContext as SQLiteContext;
                    if (SelectedService != null)
                    {
                        context.ServiceForModel.Load();
                        ServiceForModels = context.ServiceForModel.Where(s => s.Service.Rowid == SelectedService.Service.Rowid).ToList();
                        context.Model.Load();
                        Models = context.Model.Local.Where(s => ServiceForModels.All(sfm => sfm.Model.Rowid != s.Rowid)).ToList();
                    }
                }
                OnPropertyChanged("SelectedService");
                _openEditServiceWindow?.RaiseCanExecuteChanged();
                _openDialogWindow?.RaiseCanExecuteChanged();
            }
        }
        public Model SelectedModel { get { return _selectedModel; } set { _selectedModel = value; OnPropertyChanged("SelectedModel"); } }
        public ServiceForModel SelectedServiceForModel { get { return _selectedServiceForModel; } set { _selectedServiceForModel = value; OnPropertyChanged("SelectedServiceForModel"); } }
        public String Search
        {
            get { return _search; }
            set
            {
                _search = value;
                _filterSearch.SetWhat(_search); // Задание поисковой строки
                _filterSearch.SetWhere("FullName"); // Задание пути для поиска
                _filterSearch.AddWhere(_filterSearch.Member); // Добавление пути в список путей
                _filterSearch.SetWhere("ShortName"); // Задание пути для поиска
                _filterSearch.AddWhere(_filterSearch.Member); // Добавление пути в список путей
                _filterSearch.CreateFilter(); // Создание фильтра
                OnPropertyChanged("Search");
            }
        }
        public IList<SView> Services { get; set; }
        public IList<Model> Models { get { return _models; } set { _models = value; OnPropertyChanged("Models"); } }
        public IList<ServiceForModel> ServiceForModels { get { return _serviceForModels; } set { _serviceForModels = value; OnPropertyChanged("ServiceForModels"); } }
        public ObservableCollection<Model> SelectedModels
        {
            get { return (ObservableCollection<Model>)_selectedModels; }
            set
            {
                _selectedModels = value;
                OnPropertyChanged("SelectedModels");
            }
        }
        public ObservableCollection<ServiceForModel> SelectedServiceForModels
        {
            get { return (ObservableCollection<ServiceForModel>)_selectedServiceForModels; }
            set
            {
                _selectedServiceForModels = value;
                OnPropertyChanged("SelectedServiceForModels");
            }
        }
        #endregion

        #region Конструктор
        public ServiceVM()
        {
            InitializeFilters();
            Services = new ObservableCollection<SView>();
            SelectedModels = new ObservableCollection<Model>();
            SelectedServiceForModels = new ObservableCollection<ServiceForModel>();
            _dbContext = SingletonDBContext.GetInstance(new SQLiteContext()).DBContext;
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Service.Load();
                var servicesList = context.Service.Local.ToBindingList();
                ServicesListCreator(servicesList);
            }

        }
        #endregion

        #region Методы
        private void InitializeFilters()
        {
            _parameter = System.Linq.Expressions.Expression.Parameter(typeof(Service), "s");
            _filterSearch = new FilterSearch(_parameter);

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
        private void ServicesListCreator(IList<Service> list)
        {
            Services.Clear();
            foreach (var item in list)
            {
                Services.Add(new SView(item, Services.Count() + 1));
            }
        }

        public System.Collections.IList SelectedItems
        {
            set
            {
                System.Collections.IList temp = null;

                temp = ItemsBuilder.SelectItem(value, SelectedServiceForModels, typeof(ServiceForModel), SelectedServiceForModel);
                if (temp != null) SelectedServiceForModels = (ObservableCollection<ServiceForModel>)temp;

                temp = ItemsBuilder.SelectItem(value, SelectedModels, typeof(Model), SelectedModel);
                if (temp != null) SelectedModels = (ObservableCollection<Model>)temp;

                OnFilterChanged();
            }
        }

        public void OnFilterChanged()
        {
            IList<Service> tempServices = new ObservableCollection<Service>();
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
                lambda = System.Linq.Expressions.Expression.Lambda<Func<Service, bool>>(result, _parameter).Compile();
            }
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                tempServices = context.Service.ToList();
                if (lambda != null)
                    tempServices = context.Service.Where((Func<Service, bool>)lambda).ToList();
                ServicesListCreator(tempServices);
            }
            SelectedService = null;
        }
        //Обработчики для команд
        private void ExecuteAdd(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var addServiceVM = new AddServiceVM();
            displayRootRegistry.ShowPresentation(addServiceVM);
        }
        private async void ExecuteEdit(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var editServiceVM = new EditServiceVM(_selectedService.Service.Rowid);
            await displayRootRegistry.ShowModalPresentation(editServiceVM);
        }
        private void ExecuteRemove(object parameter)
        {
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Configuration.LazyLoadingEnabled = false;
                context.Service.Remove(_selectedService.Service);
                context.SaveChanges();
                SelectedService = null;
                Models.Clear();
                ServiceForModels.Clear();
                OnFilterChanged();
            }
        }
        private void ExecuteRefresh(object parameter)
        {
            OnFilterChanged();
        }
        private async void OpenDialog(object parameter)
        {
            var message = String.Format("Вы действительно хотите удалить сервис \"{0}\", и все его значения из записей?", _selectedService.Service.FullName);
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM("Удаление сервиса", message, ExecuteRemove);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        private bool CanExecuteEdit(object parameter)
        {
            if (_selectedService != null)
                return true;
            return false;
        }
        #endregion

        public class SView
        {
            public Service Service { get; private set; }
            public int Index { get; set; }

            public SView(Service service, int index)
            {
                Service = service;
                Index = index;
            }
        }
    }
}
