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
    public class CategoryVM : BaseVM
    {
        #region Поля
        private DbContext _dbContext;
        private string _search = String.Empty; //Поисковая строка

        private FilterSearch _filterSearch; //Фильтр поиска
        private List<IFilter> _filters; //Список всех фильтров
        private ParameterExpression _parameter; //Параметр для формирования лямбды фильтрации     

        private SCView _selectedServiceCategory; //Выбранная категория сложности

        private IDelegateCommand _openAddCategoryWindow; //Команда открытия окна добавления
        private IDelegateCommand _openEditCategoryWindow; //Команда открытия окна изменения
        private IDelegateCommand _refreshCategoryWindow; //Команда обновления данных в окне
        private IDelegateCommand _openDialogWindow; //Команда открытия диалогового окна
        #endregion

        #region Свойства
        //Команды для кнопок
        public IDelegateCommand AddCategoryCommand
        {
            get
            {
                if (_openAddCategoryWindow == null)
                {
                    _openAddCategoryWindow = new OpenWindowCommand(ExecuteAdd, this);
                }
                return _openAddCategoryWindow;
            }
        }
        public IDelegateCommand EditCategoryCommand
        {
            get
            {
                if (_openEditCategoryWindow == null)
                {
                    _openEditCategoryWindow = new OpenWindowCommand(ExecuteEdit, CanExecuteEdit, this);
                }
                return _openEditCategoryWindow;
            }
        }
        public IDelegateCommand RemoveCategoryCommand
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
        public IDelegateCommand RefreshCategoryCommand
        {
            get
            {
                if (_refreshCategoryWindow == null)
                {
                    _refreshCategoryWindow = new DelegateCommand(ExecuteRefresh);
                }
                return _refreshCategoryWindow;
            }
        }
        //Свойства модели
        public SCView SelectedServiceCategory { get { return _selectedServiceCategory; } set { _selectedServiceCategory = value; OnPropertyChanged("SelectedServiceCategory"); _openEditCategoryWindow?.RaiseCanExecuteChanged(); _openDialogWindow?.RaiseCanExecuteChanged(); } }
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
        public IList<SCView> ServiceCategories { get; set; }
        #endregion

        #region Конструктор
        public CategoryVM()
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
            ServiceCategories = new ObservableCollection<SCView>();
            _dbContext = SingletonDBContext.GetInstance(new SQLiteContext()).DBContext;
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.ServiceCategory.Load();
                var serviceCategoriesList = context.ServiceCategory.Local.ToBindingList();
                ServiceCategoriesListCreator(serviceCategoriesList);
            }
        }
        private void InitializeFilters()
        {
            _parameter = System.Linq.Expressions.Expression.Parameter(typeof(ServiceCategory), "s");
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
        private void ServiceCategoriesListCreator(IList<ServiceCategory> list)
        {
            ServiceCategories.Clear();
            foreach (var item in list)
            {
                ServiceCategories.Add(new SCView(item, ServiceCategories.Count() + 1));
            }
        }
       
        public void OnFilterChanged()
        {
            IList<ServiceCategory> tempServiceCategories = new ObservableCollection<ServiceCategory>();
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
                lambda = System.Linq.Expressions.Expression.Lambda<Func<ServiceCategory, bool>>(result, _parameter).Compile();
            }
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                tempServiceCategories = context.ServiceCategory.ToList();
                if (lambda != null)
                    tempServiceCategories = context.ServiceCategory.Where((Func<ServiceCategory, bool>)lambda).ToList();
                ServiceCategoriesListCreator(tempServiceCategories);
            }
            SelectedServiceCategory = null;
        }
        //Обработчики для команд
        private void ExecuteAdd(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var addCategoryVM = new AddCategoryVM();
            displayRootRegistry.ShowPresentation(addCategoryVM);
        }
        private async void ExecuteEdit(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var editCategoryVM = new EditCategoryVM(_selectedServiceCategory.ServiceCategory.Rowid);
            await displayRootRegistry.ShowModalPresentation(editCategoryVM);
        }
        private void ExecuteRemove(object parameter)
        {
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Configuration.LazyLoadingEnabled = false;
                context.ServiceCategory.Remove(_selectedServiceCategory.ServiceCategory);
                context.SaveChanges();
                SelectedServiceCategory = null;
                OnFilterChanged();
            }
        }
        private void ExecuteRefresh(object parameter)
        {
            OnFilterChanged();
        }
        private async void OpenDialog(object parameter)
        {
            var message = String.Format("Вы действительно хотите удалить категорию сложности {0}, и все связанные с ней записи?", _selectedServiceCategory.ServiceCategory.Name);
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM("Удаление устройства", message, ExecuteRemove);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        private bool CanExecuteEdit(object parameter)
        {
            if (_selectedServiceCategory != null)
                return true;
            return false;
        }
        #endregion

        public class SCView
        {
            public ServiceCategory ServiceCategory { get; private set; }
            public int Index { get; set; }

            public SCView(ServiceCategory serviceCategory, int index)
            {
                ServiceCategory = serviceCategory;
                Index = index;
            }
        }
    }
}
