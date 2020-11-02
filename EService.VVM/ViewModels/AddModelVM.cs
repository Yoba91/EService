using EService.BL;
using EService.Data.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EService.VVM.ViewModels
{
    public class AddModelVM : BaseVM
    {
        #region Поля
        private String _fullName, _shortName;
        private TypeModel _selectedTypeModel;
        private DbContext _dbContext;

        private IDelegateCommand _addModel;
        #endregion
        #region Свойства
        public String FullName { get { return _fullName; } set { _fullName = value; this.AddModel.RaiseCanExecuteChanged(); } }
        public String ShortName { get { return _shortName; } set { _shortName = value; this.AddModel.RaiseCanExecuteChanged(); } }
        public TypeModel TypeModel { get { return _selectedTypeModel; } set { _selectedTypeModel = value; this.AddModel.RaiseCanExecuteChanged(); } }
        public IList<TypeModel> TypesModel { get; set; }

        public IDelegateCommand AddModel
        {
            get
            {
                if (_addModel == null)
                {
                    _addModel = new DelegateCommand(OpenDialog, CanExecuteAdd);
                }
                return _addModel;
            }
        }

        #endregion
        #region Конструкторы
        public AddModelVM()
        {
            FullName = String.Empty;
            ShortName = String.Empty;
            TypeModel = null;
            var sdbContext = SingletonDBContext.GetInstance(new SQLiteContext());
            _dbContext = sdbContext.DBContext;
            if(_dbContext is SQLiteContext)
            {
                var context = _dbContext as SQLiteContext;
                context.TypeModel.Load();
                TypesModel = context.TypeModel.Local.ToBindingList();
            }
        }
        #endregion
        #region Методы
        private void ExecuteAdd(object parameter)
        {
            Model model = new Model()
            {
                FullName = this._fullName,
                ShortName = this._shortName,
                TypeModel = this._selectedTypeModel
            };

            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Model.Add(model);
                context.SaveChanges();
                context.Configuration.LazyLoadingEnabled = true;
                context.Database.Initialize(true);
            }
        }

        private bool CanExecuteAdd(object parameter)
        {
            if ((FullName != String.Empty) && (ShortName != String.Empty) && (TypeModel != null))
                return true;
            return false;
        }

        private async void OpenDialog(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM("Новая модель", "Вы действительно хотите добавить новую модель?", ExecuteAdd);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        #endregion
    }
}
