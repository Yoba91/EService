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
    class UserVM : BaseVM
    {
        #region Поля
        private DbContext _dbContext;
        private string _search = String.Empty; //Поисковая строка

        private FilterSearch _filterSearch; //Фильтр поиска
        private FilterId _filterServiceLog; //Фильтры по ID
        private List<IFilter> _filters; //Список всех фильтров
        private ParameterExpression _parameter, _parameterServiceLog; //Параметр для формирования лямбды фильтрации     

        private UView _selectedUser; //Выбранный пользователь
        private Model _selectedModel; //Выбранная модель
        private TypeModel _selectedTypeModel; //Выбранный тип модели

        private IList<Model> _selectedModels; //Список выбранных моделей устройств
        private IList<TypeModel> _selectedTypesModel; //Список выбранных моделей устройств

        private IList<Model> _models; //Список моделей устройств
        private IList<TypeModel> _typesModel; //Список типов моделей устройств

        private IDelegateCommand _openAddUserWindow; //Команда открытия окна добавления
        private IDelegateCommand _openEditUserWindow; //Команда открытия окна изменения
        private IDelegateCommand _refreshUserWindow; //Команда обновления данных в окне
        private IDelegateCommand _openDialogWindow; //Команда открытия диалогового окна
        #endregion

        #region Свойства
        //Команды для кнопок
        public IDelegateCommand AddUserCommand
        {
            get
            {
                if (_openAddUserWindow == null)
                {
                    _openAddUserWindow = new OpenWindowCommand(ExecuteAdd, this);
                }
                return _openAddUserWindow;
            }
        }
        public IDelegateCommand EditUserCommand
        {
            get
            {
                if (_openEditUserWindow == null)
                {
                    _openEditUserWindow = new OpenWindowCommand(ExecuteEdit, CanExecuteEdit, this);
                }
                return _openEditUserWindow;
            }
        }
        public IDelegateCommand RemoveUserCommand
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
        public IDelegateCommand RefreshUserCommand
        {
            get
            {
                if (_refreshUserWindow == null)
                {
                    _refreshUserWindow = new DelegateCommand(ExecuteRefresh);
                }
                return _refreshUserWindow;
            }
        }
        //Свойства модели
        public UView SelectedUser { get { return _selectedUser; } set { _selectedUser = value; OnPropertyChanged("SelectedUser"); _openEditUserWindow?.RaiseCanExecuteChanged(); _openDialogWindow?.RaiseCanExecuteChanged(); } }
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
                _filterSearch.SetWhere("Surname"); // Задание пути для поиска
                _filterSearch.AddWhere(_filterSearch.Member); // Добавление пути в список путей
                _filterSearch.SetWhere("Midname"); // Задание пути для поиска
                _filterSearch.AddWhere(_filterSearch.Member); // Добавление пути в список путей
                _filterSearch.SetWhere("FullName"); // Задание пути для поиска
                _filterSearch.AddWhere(_filterSearch.Member); // Добавление пути в список путей
                _filterSearch.CreateFilter(); // Создание фильтра
                OnPropertyChanged("Search");
            }
        }
        public IList<UView> Users { get; set; }
        public IList<Model> Models { get { return _models; } set { _models = value; OnPropertyChanged("Models"); } }
        public IList<TypeModel> TypesModel { get { return _typesModel; } set { _typesModel = value; OnPropertyChanged("TypesModel"); } }
        public ObservableCollection<Model> SelectedModels
        {
            get { return (ObservableCollection<Model>)_selectedModels; }
            set
            {
                _selectedModels = value;
                SetFilter(SelectedModels, _filterServiceLog,"Device", "Model", "Rowid");
                OnPropertyChanged("SelectedModels");
            }
        }
        public ObservableCollection<TypeModel> SelectedTypesModel
        {
            get { return (ObservableCollection<TypeModel>)_selectedTypesModel; }
            set
            {
                _selectedTypesModel = value;
                SetFilter(SelectedTypesModel, _filterServiceLog, "Device", "Model", "TypeModel", "Rowid");
                OnPropertyChanged("SelectedTypesModel");
            }
        }
        #endregion

        #region Конструктор
        public UserVM()
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
            Users = new ObservableCollection<UView>();
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
                context.Repairer.Load();
                var usersList = context.Repairer.Local.ToBindingList();
                UsersListCreator(usersList, null);
            }
        }
        private void InitializeFilters()
        {
            _parameter = System.Linq.Expressions.Expression.Parameter(typeof(Repairer), "s");
            _parameterServiceLog = System.Linq.Expressions.Expression.Parameter(typeof(ServiceLog), "s");
            _filterSearch = new FilterSearch(_parameter);
            _filterServiceLog = new FilterId(_parameterServiceLog);

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
        private void UsersListCreator(IList<Repairer> list, Delegate lambdaServiceLog)
        {
            Users.Clear();
            foreach (var item in list)
            {
                var serviceLogsCount = item.ServiceLogs.Count();
                if (lambdaServiceLog != null)
                    serviceLogsCount = item.ServiceLogs.Where((Func<ServiceLog, bool>)lambdaServiceLog).Count();
                Users.Add(new UView(item, serviceLogsCount, Users.Count() + 1));
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
            IList<Repairer> tempUsers = new ObservableCollection<Repairer>();
            System.Linq.Expressions.Expression result = null, temp;
            Delegate lambda = null, lambdaS = null;
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
                lambda = System.Linq.Expressions.Expression.Lambda<Func<Repairer, bool>>(result, _parameter).Compile();
            }
            if (_filterServiceLog.GetFilter() != null)
                lambdaS = System.Linq.Expressions.Expression.Lambda<Func<ServiceLog, bool>>(_filterServiceLog.GetFilter(), _parameterServiceLog).Compile();
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                tempUsers = context.Repairer.ToList();
                if (lambda != null)
                    tempUsers = context.Repairer.Where((Func<Repairer, bool>)lambda).ToList();
                if (lambdaS != null)
                    tempUsers = tempUsers.Where(r => r.ServiceLogs.Where((Func<ServiceLog, bool>)lambdaS).Count() > 0).ToList();
                UsersListCreator(tempUsers, lambdaS);
            }
            SelectedUser = null;
        }
        //Обработчики для команд
        private void ExecuteAdd(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var addUserVM = new AddUserVM();
            displayRootRegistry.ShowPresentation(addUserVM);
        }
        private async void ExecuteEdit(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var editUserVM = new EditUserVM(_selectedUser.User.Rowid);
            await displayRootRegistry.ShowModalPresentation(editUserVM);
        }
        private void ExecuteRemove(object parameter)
        {
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Configuration.LazyLoadingEnabled = false;
                context.Repairer.Remove(_selectedUser.User);
                context.SaveChanges();
                SelectedUser = null;
                OnFilterChanged();
            }
        }
        private void ExecuteRefresh(object parameter)
        {
            OnFilterChanged();
        }
        private async void OpenDialog(object parameter)
        {
            var message = String.Format("Вы действительно хотите удалить пользователя \"{0}\", и все связанные с ним записи?", _selectedUser.User.FullName);
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM("Удаление пользователя", message, ExecuteRemove);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        private bool CanExecuteEdit(object parameter)
        {
            if (_selectedUser != null)
                return true;
            return false;
        }
        #endregion
        public class UView
        {
            public Repairer User { get; private set; }
            public int ServiceLogsCount { get; set; }
            public int Index { get; set; }

            public UView(Repairer user, int serviceLogsCount, int index)
            {
                User = user;
                ServiceLogsCount = serviceLogsCount;
                Index = index;
            }
        }
    }
}
