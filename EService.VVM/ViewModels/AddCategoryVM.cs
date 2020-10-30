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
    public class AddCategoryVM : BaseVM
    {
        #region Поля
        private String _name, _range;
        private int _minValue, _maxValue;
        private DbContext _dbContext;

        private IDelegateCommand _addCategory;
        #endregion
        #region Свойства
        public String Name { get { return _name; } set { _name = value; this.AddCategory.RaiseCanExecuteChanged(); } }
        public String Range { get { return _range; } set { _range = value; } }
        public int MinValue { get { return _minValue; } set { _minValue = value; } }
        public int MaxValue { get { return _maxValue; } set { _maxValue = value; } }
        public IDelegateCommand AddCategory
        {
            get
            {
                if (_addCategory == null)
                {
                    _addCategory = new DelegateCommand(OpenDialog, CanExecuteAdd);
                }
                return _addCategory;
            }
        }

        #endregion
        #region Конструкторы
        public AddCategoryVM()
        {
            Name = String.Empty;
            Range = String.Empty;
            var sdbContext = SingletonDBContext.GetInstance(new SQLiteContext());
            _dbContext = sdbContext.DBContext;
        }
        #endregion
        #region Методы
        private void ExecuteAdd(object parameter)
        {
            ServiceCategory serviceCategory = new ServiceCategory()
            {
                Name = this.Name,
                Range = String.Format(this.MinValue.ToString() + "_" + this.MaxValue.ToString())
            };

            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.ServiceCategory.Add(serviceCategory);
                context.SaveChanges();
            }
        }

        private bool CanExecuteAdd(object parameter)
        {
            if (Name != String.Empty)
                return true;
            return false;
        }

        private async void OpenDialog(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM("Новая категория сложности", "Вы действительно хотите добавить новую категорию сложности ремонта?", ExecuteAdd);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        #endregion
    }
}

