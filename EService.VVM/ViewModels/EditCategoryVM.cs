using EService.BL;
using EService.Data.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EService.VVM.ViewModels
{
    public class EditCategoryVM : BaseVM
    {
        #region Поля
        private String _name, _range;
        private int _minValue, _maxValue;
        private DbContext _dbContext;
        private ServiceCategory _serviceCategory;

        private IDelegateCommand _editCategory;
        #endregion
        #region Свойства
        public String Name { get { return _name; } set { _name = value; this.EditCategory.RaiseCanExecuteChanged(); } }
        public String Range { get { return _range; } set { _range = value; } }
        public int MinValue { get { return _minValue; } set { _minValue = value; this.EditCategory.RaiseCanExecuteChanged(); } }
        public int MaxValue { get { return _maxValue; } set { _maxValue = value; this.EditCategory.RaiseCanExecuteChanged(); } }
        public IDelegateCommand EditCategory
        {
            get
            {
                if (_editCategory == null)
                {
                    _editCategory = new DelegateCommand(OpenDialog, CanExecuteEdit);
                }
                return _editCategory;
            }
        }
        #endregion
        #region Конструкторы
        public EditCategoryVM(long rowid)
        {
            Name = String.Empty;
            Range = String.Empty;
            var sdbContext = SingletonDBContext.GetInstance(new SQLiteContext());
            _dbContext = sdbContext.DBContext;
            if (_dbContext is SQLiteContext)
            {
                var context = _dbContext as SQLiteContext;
                _serviceCategory = context.ServiceCategory.Where(s => s.Rowid == rowid).SingleOrDefault();
                Name = _serviceCategory.Name;
                Range = _serviceCategory.Range;
                MinValue = _serviceCategory.MinValue;
                MaxValue = _serviceCategory.MaxValue;
            }
        }
        #endregion
        #region Методы
        private void ExecuteEdit(object parameter)
        {
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                ServiceCategory serviceCategory = context.ServiceCategory.Where(s => s.Rowid == _serviceCategory.Rowid).SingleOrDefault();
                serviceCategory.Name = Name;
                serviceCategory.Range = String.Format(this.MinValue.ToString() + "_" + this.MaxValue.ToString());
                context.SaveChanges();
            }
        }

        private bool CanExecuteEdit(object parameter)
        {
            if (Name != String.Empty)
                return true;
            return false;
        }

        private async void OpenDialog(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM(String.Format("Изменить категорию сложности {0}", _serviceCategory.Name), "Вы действительно хотите изменить выбранную категорию сложности?", ExecuteEdit);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        #endregion
    }
}
