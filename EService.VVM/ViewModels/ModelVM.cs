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
    public class ModelVM : BaseVM
    {
        #region Поля
        private DbContext _dbContext;
        private string _search = String.Empty; //Поисковая строка

        private FilterSearch _filterSearch; //Фильтр поиска
        private FilterId _filterDept, _filterTypeModel; //Фильтры по ID
        private List<IFilter> _filters; //Список всех фильтров
        private ParameterExpression _parameter, _parameterDept; //Параметр для формирования лямбды фильтрации     

        private Dept _selectedDept; //Выбранный отдел
        private TypeModel _selectedTypeModel; //Выбранный тип модели
        private MView _selectedModel; //Выбранная модель

        private IList<Dept> _selectedDepts; //Список выбранных отделов
        private IList<TypeModel> _selectedTypesModel; //Список выбранных типов моделей устройств

        private IDelegateCommand _openAddModelWindow; //Команда открытия окна добавления
        private IDelegateCommand _openEditModelWindow; //Команда открытия окна изменения
        private IDelegateCommand _refreshModelWindow; //Команда обновления данных в окне
        private IDelegateCommand _openDialogWindow; //Команда открытия диалогового окна
        #endregion

        #region Свойства
        //Команды для кнопок
        public IDelegateCommand AddModelCommand
        {
            get
            {
                if (_openAddModelWindow == null)
                {
                    _openAddModelWindow = new OpenWindowCommand(ExecuteAdd, this);
                }
                return _openAddModelWindow;
            }
        }
        public IDelegateCommand EditModelCommand
        {
            get
            {
                if (_openEditModelWindow == null)
                {
                    _openEditModelWindow = new OpenWindowCommand(ExecuteEdit, CanExecuteEdit, this);
                }
                return _openEditModelWindow;
            }
        }
        public IDelegateCommand RemoveModelCommand
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
        public IDelegateCommand RefreshModelCommand
        {
            get
            {
                if (_refreshModelWindow == null)
                {
                    _refreshModelWindow = new DelegateCommand(ExecuteRefresh);
                }
                return _refreshModelWindow;
            }
        }
        //Свойства модели
        public MView SelectedModel { get { return _selectedModel; } set { _selectedModel = value; OnPropertyChanged("SelectedModel"); _openEditModelWindow?.RaiseCanExecuteChanged(); _openDialogWindow?.RaiseCanExecuteChanged(); } }
        public Dept SelectedDept { get { return _selectedDept; } set { _selectedDept = value; OnPropertyChanged("SelectedDept"); } }        
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
            Models = new ObservableCollection<MView>();
            SelectedTypesModel = new ObservableCollection<TypeModel>();
            SelectedDepts = new ObservableCollection<Dept>();
            _dbContext = SingletonDBContext.GetInstance(new SQLiteContext()).DBContext;
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.TypeModel.Load();
                TypesModel = context.TypeModel.Local.ToBindingList();
                context.Dept.Load();
                Depts = context.Dept.Local.ToBindingList();
                context.Model.Load();
                var modelsList = context.Model.Local.ToBindingList();
                ModelsListCreator(modelsList, null);
            }
        }
        private void InitializeFilters()
        {
            _parameter = System.Linq.Expressions.Expression.Parameter(typeof(Model), "s");
            _parameterDept = System.Linq.Expressions.Expression.Parameter(typeof(Device), "s");
            _filterSearch = new FilterSearch(_parameter);
            _filterDept = new FilterId(_parameterDept);
            _filterTypeModel = new FilterId(_parameter);

            _filters = new List<IFilter>();

            _filters.Add(_filterSearch);
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
            foreach (var item in list)
            {
                int devicesCount = item.Devices.Count();
                if (lambdaDept != null)
                {
                    devicesCount = item.Devices.Where((Func<Device, bool>)lambdaDept).Count();
                }
                Models.Add(new MView(item, devicesCount, Models.Count() + 1));
            }
        }

        public System.Collections.IList SelectedItems
        {
            set
            {
                System.Collections.IList temp = null;

                temp = ItemsBuilder.SelectItem(value, SelectedTypesModel, typeof(TypeModel), SelectedTypeModel);
                if (temp != null) SelectedTypesModel = (ObservableCollection<TypeModel>)temp;

                temp = ItemsBuilder.SelectItem(value, SelectedDepts, typeof(Dept), SelectedDept);
                if (temp != null) SelectedDepts = (ObservableCollection<Dept>)temp;

                

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
            SelectedModel = null;
        }
        //Обработчики для команд
        private void ExecuteAdd(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var addModelVM = new AddModelVM();
            displayRootRegistry.ShowPresentation(addModelVM);
        }
        private async void ExecuteEdit(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var editModelVM = new EditModelVM(_selectedModel.Model.Rowid);
            await displayRootRegistry.ShowModalPresentation(editModelVM);
        }
        private void ExecuteRemove(object parameter)
        {
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Configuration.LazyLoadingEnabled = false;
                context.Model.Remove(_selectedModel.Model);
                context.SaveChanges();
                SelectedModel = null;
                OnFilterChanged();
            }
        }
        private void ExecuteRefresh(object parameter)
        {
            OnFilterChanged();
        }
        private async void OpenDialog(object parameter)
        {
            var message = String.Format("Вы действительно хотите удалить модель устройства {0}({1}), и все связанные с ним устройства и записи?", _selectedModel.Model.FullName, _selectedModel.Model.ShortName);
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM("Удаление модели", message, ExecuteRemove);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        private bool CanExecuteEdit(object parameter)
        {
            if (_selectedModel != null)
                return true;
            return false;
        }
        #endregion

        public class MView
        {
            public Model Model { get; private set; }
            public int DevicesCount { get; private set; }
            public int Index { get; set; }

            public MView(Model model, int devicesCount, int index)
            {
                Model = model;
                DevicesCount = devicesCount;
                Index = index;
            }
        }
    }
}
