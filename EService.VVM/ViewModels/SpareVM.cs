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
    class SpareVM : BaseVM
    {
        #region Поля
        private DbContext _dbContext;
        private string _search = String.Empty; //Поисковая строка

        private FilterSearch _filterSearch; //Фильтр поиска
        private List<IFilter> _filters; //Список всех фильтров
        private ParameterExpression _parameter; //Параметр для формирования лямбды фильтрации     

        private SView _selectedSpare; //Выбранная запчасть
        private Model _selectedModel; //Выбранная модель
        private SpareForModel _selectedSpareForModel; //Выбранная запчасть

        private IList<Model> _selectedModels; //Список выбранных моделей устройств
        private IList<SpareForModel> _selectedSpareForModels; //Список выбранных запчастей привязанных к модели

        private IList<Model> _models; //Список моделей устройств
        private IList<SpareForModel> _spareForModels; //Список запчастей привязанных к модели

        private IDelegateCommand _openAddSpareWindow; //Команда открытия окна добавления записи в журнал
        private IDelegateCommand _openEditSpareWindow; //Команда открытия окна изменения записи в журнале
        private IDelegateCommand _refreshSpareWindow; //Команда обновления данных в окне
        private IDelegateCommand _openDialogWindow; //Команда открытия диалогового окна
        #endregion

        #region Свойства
        //Команды для кнопок
        public IDelegateCommand AddSpareCommand
        {
            get
            {
                if (_openAddSpareWindow == null)
                {
                    _openAddSpareWindow = new OpenWindowCommand(ExecuteAdd, this);
                }
                return _openAddSpareWindow;
            }
        }
        public IDelegateCommand EditSpareCommand
        {
            get
            {
                if (_openEditSpareWindow == null)
                {
                    _openEditSpareWindow = new OpenWindowCommand(ExecuteEdit, CanExecuteEdit, this);
                }
                return _openEditSpareWindow;
            }
        }
        public IDelegateCommand RemoveSpareCommand
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
        public IDelegateCommand RefreshSpareCommand
        {
            get
            {
                if (_refreshSpareWindow == null)
                {
                    _refreshSpareWindow = new DelegateCommand(ExecuteRefresh);
                }
                return _refreshSpareWindow;
            }
        }
        //Свойства модели
        public SView SelectedSpare
        {
            get
            {
                return _selectedSpare;
            }
            set
            {
                _selectedSpare = value;
                if (_dbContext is SQLiteContext)
                {
                    SQLiteContext context = _dbContext as SQLiteContext;
                    if (SelectedSpare != null)
                    {
                        context.SpareForModel.Load();
                        SpareForModels = context.SpareForModel.Where(s => s.Spare.Rowid == SelectedSpare.Spare.Rowid).ToList();
                        context.Model.Load();
                        Models = context.Model.Local.Where(s => SpareForModels.All(sfm => sfm.Model.Rowid != s.Rowid)).ToList();
                    }
                }
                OnPropertyChanged("SelectedSpare");
                _openEditSpareWindow?.RaiseCanExecuteChanged();
                _openDialogWindow?.RaiseCanExecuteChanged();
            }
        }
        public Model SelectedModel { get { return _selectedModel; } set { _selectedModel = value; OnPropertyChanged("SelectedModel"); } }
        public SpareForModel SelectedSpareForModel { get { return _selectedSpareForModel; } set { _selectedSpareForModel = value; OnPropertyChanged("SelectedSpareForModel"); } }
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
        public IList<SView> Spares { get; set; }
        public IList<Model> Models { get { return _models; } set { _models = value; OnPropertyChanged("Models"); } }
        public IList<SpareForModel> SpareForModels { get { return _spareForModels; } set { _spareForModels = value; OnPropertyChanged("SpareForModels"); } }
        public ObservableCollection<Model> SelectedModels
        {
            get { return (ObservableCollection<Model>)_selectedModels; }
            set
            {
                _selectedModels = value;
                OnPropertyChanged("SelectedModels");
            }
        }
        public ObservableCollection<SpareForModel> SelectedSpareForModels
        {
            get { return (ObservableCollection<SpareForModel>)_selectedSpareForModels; }
            set
            {
                _selectedSpareForModels = value;
                OnPropertyChanged("SelectedSpareForModels");
            }
        }
        #endregion

        #region Конструктор
        public SpareVM()
        {
            InitializeFilters();
            Spares = new ObservableCollection<SView>();
            SelectedModels = new ObservableCollection<Model>();
            SelectedSpareForModels = new ObservableCollection<SpareForModel>();
            _dbContext = SingletonDBContext.GetInstance(new SQLiteContext()).DBContext;
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Spare.Load();
                var sparesList = context.Spare.Local.ToBindingList();
                SparesListCreator(sparesList);
            }

        }
        #endregion

        #region Методы
        private void InitializeFilters()
        {
            _parameter = System.Linq.Expressions.Expression.Parameter(typeof(Spare), "s");
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
        private void SparesListCreator(IList<Spare> list)
        {
            Spares.Clear();
            foreach (var item in list)
            {
                Spares.Add(new SView(item, Spares.Count() + 1));
            }
        }

        public System.Collections.IList SelectedItems
        {
            set
            {
                System.Collections.IList temp = null;

                temp = ItemsBuilder.SelectItem(value, SelectedSpareForModels, typeof(SpareForModel), SelectedSpareForModel);
                if (temp != null) SelectedSpareForModels = (ObservableCollection<SpareForModel>)temp;

                temp = ItemsBuilder.SelectItem(value, SelectedModels, typeof(Model), SelectedModel);
                if (temp != null) SelectedModels = (ObservableCollection<Model>)temp;

                OnFilterChanged();
            }
        }

        public void OnFilterChanged()
        {
            IList<Spare> tempSpares = new ObservableCollection<Spare>();
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
                lambda = System.Linq.Expressions.Expression.Lambda<Func<Spare, bool>>(result, _parameter).Compile();
            }
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                tempSpares = context.Spare.ToList();
                if (lambda != null)
                    tempSpares = context.Spare.Where((Func<Spare, bool>)lambda).ToList();
                SparesListCreator(tempSpares);
            }
            SelectedSpare = null;
        }
        //Обработчики для команд
        private void ExecuteAdd(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var addSpareVM = new AddSpareVM();
            displayRootRegistry.ShowPresentation(addSpareVM);
        }
        private async void ExecuteEdit(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var editSpareVM = new EditSpareVM(_selectedSpare.Spare.Rowid);
            await displayRootRegistry.ShowModalPresentation(editSpareVM);
        }
        private void ExecuteRemove(object parameter)
        {
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Configuration.LazyLoadingEnabled = false;
                context.Spare.Remove(_selectedSpare.Spare);
                context.SaveChanges();
                SelectedSpare = null;
                Models.Clear();
                SpareForModels.Clear();
                OnFilterChanged();
            }
        }
        private void ExecuteRefresh(object parameter)
        {
            OnFilterChanged();
        }
        private async void OpenDialog(object parameter)
        {
            var message = String.Format("Вы действительно хотите удалить запчасть \"{0}\", и все её значения из записей?", _selectedSpare.Spare.Name);
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM("Удаление запчасти", message, ExecuteRemove);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        private bool CanExecuteEdit(object parameter)
        {
            if (_selectedSpare != null)
                return true;
            return false;
        }
        #endregion
        public class SView
        {
            public Spare Spare { get; private set; }
            public int Index { get; set; }

            public SView(Spare spare, int index)
            {
                Spare = spare;
                Index = index;
            }
        }
    }
}
