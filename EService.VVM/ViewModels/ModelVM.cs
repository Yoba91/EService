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
    public class ModelVM : INotifyPropertyChanged
    {
        #region Поля
        private DbContext _dbContext;
        private string _search = String.Empty; //Поисковая строка

        private FilterSearch _filterSearch; //Фильтр поиска
        private FilterId _filterDept, _filterTypeModel; //Фильтры по ID
        private List<IFilter> _filters; //Список всех фильтров
        private ParameterExpression _parameter, _parameterTypeModel, _parameterDept; //Параметр для формирования лямбды фильтрации     

        private Dept _selectedDept; //Выбранный отдел
        private TypeModel _selectedTypeModel; //Выбранный тип модели
        private MView _selectedModel; //Выбранная модель

        private IList<Dept> _selectedDepts; //Список выбранных отделов
        private IList<TypeModel> _selectedTypesModel; //Список выбранных типов моделей устройств

        private IDelegateCommand _openAddModelWindow; //Команда открытия окна добавления записи в журнал
        private IDelegateCommand _openEditModelWindow; //Команда открытия окна изменения записи в журнале
        private IDelegateCommand _refreshModelWindow; //Команда обновления данных в окне
        private IDelegateCommand _openDialogWindow; //Команда открытия диалогового окна
        #endregion

        #region Свойства
        public Dept SelectedDept { get { return _selectedDept; } set { _selectedDept = value; OnPropertyChanged("SelectedDept"); } }
        public MView SelectedModel { get { return _selectedModel; } set { _selectedModel = value; OnPropertyChanged("SelectedModel"); } }
        public TypeModel SelectedTypeModel { get { return _selectedTypeModel; } set { _selectedTypeModel = value; OnPropertyChanged("SelectedTypeModel"); } }
        public String Search
        {
            get { return _search; }
            set
            {
                _search = value;
                _filterSearch.SetWhat(_search); // Задание поисковой строки
                _filterSearch.SetWhere("FullName"); // Задание пути для поиска
                _filterSearch.AddWhere(_filterSearch.Member); // Добавление пути в список путей
                _filterSearch.SetWhere("ShortName"); // Задание второго пути поиска
                _filterSearch.AddWhere(_filterSearch.Member); // Добавление второго пути в список путей
                _filterSearch.CreateFilter(); // Создание фильтра
                OnPropertyChanged("Search");
            }
        }
        public IList<MView> Models { get; set; }
        public IList<Dept> Depts { get; set; }
        public IList<TypeModel> TypesModel { get; set; }
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
        public ObservableCollection<TypeModel> SelectedTypesModel
        {
            get { return (ObservableCollection<TypeModel>)_selectedTypesModel; }
            set
            {
                _selectedTypesModel = value;
                SetFilter(SelectedTypesModel, _filterTypeModel, "TypeModel", "Rowid");
                OnPropertyChanged("SelectedTypesModel");
            }
        }
        #endregion

        #region Конструктор
        public ModelVM()
        {
            InitializeFilters();
            Models = new ObservableCollection<MView>();
            SelectedDepts = new ObservableCollection<Dept>();
            SelectedTypesModel = new ObservableCollection<TypeModel>();
            _dbContext = SingletonDBContext.GetInstance(new SQLiteContext()).DBContext;
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Dept.Load();
                Depts = context.Dept.Local.ToBindingList();
                context.TypeModel.Load();
                TypesModel = context.TypeModel.Local.ToBindingList();                
                context.Model.Load();
                var modelsList = context.Model.Local.ToBindingList();
                ModelsListCreator(modelsList, null);
                //TypesModel.Where(tm => tm.Models.Where(m => m.Devices.Where(d => d.Dept.Rowid == 1).Count() > 0).Count() > 0).Count();
                //TypesModel.Where(tm => tm.Models.Where(m => m.Rowid == 1).Count() > 0).ToList();
                //TypesModel.Where(tm => tm.Models.Where(m => m.Devices.Where(d => d.Dept.Rowid == 1).Count() > 0).Count() > 0).ToList();
                //TypesModel.Where(tm => tm.Models.Where(m => m.Rowid == 1).Count() > 0).Count();
            }

        }
        #endregion

        #region Методы
        private void InitializeFilters()
        {
            _parameter = System.Linq.Expressions.Expression.Parameter(typeof(Model), "s");
            _parameterTypeModel = System.Linq.Expressions.Expression.Parameter(typeof(Model), "s");
            _parameterDept = System.Linq.Expressions.Expression.Parameter(typeof(Device), "s");
            _filterSearch = new FilterSearch(_parameter);
            _filterDept = new FilterId(_parameterDept);
            _filterTypeModel = new FilterId(_parameter);

            _filters = new List<IFilter>();

            _filters.Add(_filterSearch);
            //_filters.Add(_filterDept);
            _filters.Add(_filterTypeModel);

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

        private void ModelsListCreator(IList<Model> list, Delegate lambdaDept)
        {
            Models.Clear();
            IList<TypeModel> typesModel = new ObservableCollection<TypeModel>();
            foreach (var item in list)
            {
                int devicesCount = item.Devices.Count();
                if (lambdaDept != null)
                {
                    devicesCount = item.Devices.Where((Func<Device, bool>)lambdaDept).Count();
                }
                Models.Add(new MView(item, devicesCount));
            }
        }

        public System.Collections.IList SelectedItems
        {
            set
            {
                System.Collections.IList temp = null;

                temp = ItemsBuilder.SelectItem(value, SelectedDepts, typeof(Dept), SelectedDept);
                if (temp != null) SelectedDepts = (ObservableCollection<Dept>)temp;

                temp = ItemsBuilder.SelectItem(value, SelectedTypesModel, typeof(TypeModel), SelectedTypeModel);
                if (temp != null) SelectedTypesModel = (ObservableCollection<TypeModel>)temp;

                OnFilterChanged();
            }
        }

        public void OnFilterChanged()
        {
            IList<Model> tempModels = new ObservableCollection<Model>();
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
                lambda = System.Linq.Expressions.Expression.Lambda<Func<Model, bool>>(result, _parameter).Compile();
            }
            if (_filterDept.GetFilter() != null)
                lambdaD = System.Linq.Expressions.Expression.Lambda<Func<Device, bool>>(_filterDept.GetFilter(), _parameterDept).Compile();
            //if (_filterTypeModel.GetFilter() != null)
            //    lambdaM = System.Linq.Expressions.Expression.Lambda<Func<Model, bool>>(_filterTypeModel.GetFilter(), _parameterTypeModel).Compile();
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                tempModels = context.Model.ToList();
                if (lambda != null)
                    tempModels = context.Model.Where((Func<Model, bool>)lambda).ToList();
                if (lambdaD != null)
                    tempModels = tempModels.Where(m => m.Devices.Where((Func<Device, bool>)lambdaD).Count() > 0).ToList();
                ModelsListCreator(tempModels, lambdaD);
            }
            SelectedTypeModel = null;
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public class MView
        {
            public Model Model { get; private set; }
            public int DevicesCount { get; private set; }

            public MView(Model model, int devicesCount)
            {
                Model = model;
                DevicesCount = devicesCount;
            }
        }
    }
}
