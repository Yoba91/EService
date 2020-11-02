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
    public class TypeModelVM : BaseVM
    {
        #region Поля
        private DbContext _dbContext;
        private string _search = String.Empty; //Поисковая строка

        private FilterSearch _filterSearch; //Фильтр поиска
        private FilterId _filterDept, _filterModel; //Фильтры по ID
        private List<IFilter> _filters; //Список всех фильтров
        private ParameterExpression _parameter,_parameterModel,_parameterDept; //Параметр для формирования лямбды фильтрации     

        private Dept _selectedDept; //Выбранный отдел
        private TMView _selectedTypeModel; //Выбранный тип устройства
        private Model _selectedModel; //Выбранная модель

        private IList<Dept> _selectedDepts; //Список выбранных отделов
        private IList<Model> _selectedModels; //Список выбранных моделей устройств

        private IDelegateCommand _openAddTypeModelWindow; //Команда открытия окна добавления
        private IDelegateCommand _openEditTypeModelWindow; //Команда открытия окна изменения
        private IDelegateCommand _refreshTypeModelWindow; //Команда обновления данных в окне
        private IDelegateCommand _openDialogWindow; //Команда открытия диалогового окна
        #endregion

        #region Свойства
        //Команды для кнопок
        public IDelegateCommand AddTypeModelCommand
        {
            get
            {
                if (_openAddTypeModelWindow == null)
                {
                    _openAddTypeModelWindow = new OpenWindowCommand(ExecuteAdd, this);
                }
                return _openAddTypeModelWindow;
            }
        }
        public IDelegateCommand EditTypeModelCommand
        {
            get
            {
                if (_openEditTypeModelWindow == null)
                {
                    _openEditTypeModelWindow = new OpenWindowCommand(ExecuteEdit, CanExecuteEdit, this);
                }
                return _openEditTypeModelWindow;
            }
        }
        public IDelegateCommand RemoveTypeModelCommand
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
        public IDelegateCommand RefreshTypeModelCommand
        {
            get
            {
                if (_refreshTypeModelWindow == null)
                {
                    _refreshTypeModelWindow = new DelegateCommand(ExecuteRefresh);
                }
                return _refreshTypeModelWindow;
            }
        }
        //Свойства модели
        public Dept SelectedDept { get { return _selectedDept; } set { _selectedDept = value; OnPropertyChanged("SelectedDept"); } }
        public TMView SelectedTypeModel { get { return _selectedTypeModel; } set { _selectedTypeModel = value; OnPropertyChanged("SelectedTypeModel"); _openEditTypeModelWindow?.RaiseCanExecuteChanged(); _openDialogWindow?.RaiseCanExecuteChanged(); } }
        public Model SelectedModel { get { return _selectedModel; } set { _selectedModel = value; OnPropertyChanged("SelectedModel"); } }
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
        public IList<TMView> TypesModel { get; set; }
        public IList<Dept> Depts { get; set; }
        public IList<Model> Models { get; set; }
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
                SetFilter(SelectedModels, _filterModel, "Rowid");
                OnPropertyChanged("SelectedModels");
            }
        }
        #endregion

        #region Конструктор
        public TypeModelVM()
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
            TypesModel = new ObservableCollection<TMView>();
            SelectedDepts = new ObservableCollection<Dept>();
            SelectedModels = new ObservableCollection<Model>();
            _dbContext = SingletonDBContext.GetInstance(new SQLiteContext()).DBContext;
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Dept.Load();
                Depts = context.Dept.Local.ToBindingList();
                context.TypeModel.Load();
                var typesModelList = context.TypeModel.Local.ToBindingList();
                TypesModelListCreator(typesModelList, null, null);
                context.Model.Load();
                Models = context.Model.Local.ToBindingList();
            }
        }
        private void InitializeFilters()
        {
            _parameter = System.Linq.Expressions.Expression.Parameter(typeof(TypeModel), "s");
            _parameterModel = System.Linq.Expressions.Expression.Parameter(typeof(Model), "s");
            _parameterDept = System.Linq.Expressions.Expression.Parameter(typeof(Device), "s");
            _filterSearch = new FilterSearch(_parameter);
            _filterDept = new FilterId(_parameterDept);
            _filterModel = new FilterId(_parameterModel);

            _filters = new List<IFilter>();

            _filters.Add(_filterSearch);
            //_filters.Add(_filterDept);
            //_filters.Add(_filterModel);

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

        private void TypesModelListCreator(IList<TypeModel> list, Delegate lambdaDept, Delegate lambdaModel)
        {
            TypesModel.Clear();
            IList<Model> models = new ObservableCollection<Model>();
            foreach (var item in list)
            {
                int modelsCount = item.Models.Count();
                models = item.Models.ToList();
                var intList = models.Select(m => m.Devices.Where(d => d.Rowid != 0));
                if (lambdaModel != null)
                {
                    models = item.Models.Where((Func<Model, bool>)lambdaModel).ToList();
                    intList = models.Select(m => m.Devices.Where(d => d.Rowid != 0));
                    modelsCount = models.Count();
                }
                if (lambdaDept != null)
                {
                    intList = models.Select(m => m.Devices.Where((Func<Device, bool>)lambdaDept));
                    modelsCount = models.Where(m => m.Devices.Where((Func<Device, bool>)lambdaDept).Count() > 0).Count();
                }
                

                int devicesCount = 0;
                foreach (var itemInt in intList)
                {
                    devicesCount += itemInt.Count();
                }
                TypesModel.Add(new TMView(item, modelsCount, devicesCount));
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

                OnFilterChanged();
            }
        }

        public void OnFilterChanged()
        {
            IList<TypeModel> tempTypesModel = new ObservableCollection<TypeModel>(); 
            System.Linq.Expressions.Expression result = null, temp;
            Delegate lambda = null, lambdaD = null, lambdaM = null;
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
                lambda = System.Linq.Expressions.Expression.Lambda<Func<TypeModel, bool>>(result, _parameter).Compile();
            }
            if (_filterDept.GetFilter() != null)
                lambdaD = System.Linq.Expressions.Expression.Lambda<Func<Device, bool>>(_filterDept.GetFilter(), _parameterDept).Compile();
            if (_filterModel.GetFilter() != null)
                lambdaM = System.Linq.Expressions.Expression.Lambda<Func<Model, bool>>(_filterModel.GetFilter(), _parameterModel).Compile();
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                tempTypesModel = context.TypeModel.ToList();
                if(lambda != null)
                    tempTypesModel = context.TypeModel.Where((Func<TypeModel, bool>)lambda).ToList();
                if (lambdaD != null)
                    tempTypesModel = tempTypesModel.Where(tm => tm.Models.Where(m => m.Devices.Where((Func<Device, bool>)lambdaD).Count() > 0).Count() > 0).ToList();
                if (lambdaM != null)
                    tempTypesModel = tempTypesModel.Where(tm => tm.Models.Where((Func<Model, bool>)lambdaM).Count() > 0).ToList();
                TypesModelListCreator(tempTypesModel,lambdaD,lambdaM);
            }
            SelectedTypeModel = null;
        }
        //Обработчики для команд
        private void ExecuteAdd(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var addTypeModelVM = new AddTypeModelVM();
            displayRootRegistry.ShowPresentation(addTypeModelVM);
        }
        private async void ExecuteEdit(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var editTypeModelVM = new EditTypeModelVM(_selectedTypeModel.TypeModel.Rowid);
            await displayRootRegistry.ShowModalPresentation(editTypeModelVM);
        }
        private void ExecuteRemove(object parameter)
        {
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Configuration.LazyLoadingEnabled = false;
                context.TypeModel.Remove(_selectedTypeModel.TypeModel);
                context.SaveChanges();
                SelectedTypeModel = null;
                OnFilterChanged();
            }
        }
        private void ExecuteRefresh(object parameter)
        {
            OnFilterChanged();
        }
        private async void OpenDialog(object parameter)
        {
            var message = String.Format("Вы действительно хотите удалить тип {0}, и все связанные с ним устройства и записи?", _selectedTypeModel.TypeModel.FullName);
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM("Удаление типа", message, ExecuteRemove);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        private bool CanExecuteEdit(object parameter)
        {
            if (_selectedTypeModel != null)
                return true;
            return false;
        }
        #endregion

        public class TMView
        {
            public TypeModel TypeModel { get; private set; }
            public int ModelsCount { get; private set; }
            public int DevicesCount { get; private set; }

            public TMView(TypeModel typeModel, int modelsCount, int devicesCount)
            {
                TypeModel = typeModel;
                ModelsCount = modelsCount;
                DevicesCount = devicesCount;
            }
        }
    }
}
