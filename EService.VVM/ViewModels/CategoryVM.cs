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
    public class CategoryVM : BaseVM
    {
        #region Поля
        private DbContext _dbContext;
        private string _search = String.Empty; //Поисковая строка

        private FilterSearch _filterSearch; //Фильтр поиска
        private List<IFilter> _filters; //Список всех фильтров
        private ParameterExpression _parameter; //Параметр для формирования лямбды фильтрации     

        private SCView _selectedServiceCategory; //Выбранная категория сложности

        private IDelegateCommand _openAddTypeModelWindow; //Команда открытия окна добавления записи в журнал
        private IDelegateCommand _openEditTypeModelWindow; //Команда открытия окна изменения записи в журнале
        private IDelegateCommand _refreshTypeModelWindow; //Команда обновления данных в окне
        private IDelegateCommand _openDialogWindow; //Команда открытия диалогового окна
        #endregion

        #region Свойства
        public SCView SelectedServiceCategory { get { return _selectedServiceCategory; } set { _selectedServiceCategory = value; OnPropertyChanged("SelectedServiceCategory"); } }
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
        #endregion

        #region Методы
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
