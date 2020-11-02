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
    public class AddParameterVM : BaseVM
    {
        #region Поля
        private String _name, _default, _unit;
        private DbContext _dbContext;

        private IDelegateCommand _addParameter;
        #endregion
        #region Свойства
        public String Name { get { return _name; } set { _name = value; this.AddParameter.RaiseCanExecuteChanged(); } }
        public String Default { get { return _default; } set { _default = value; this.AddParameter.RaiseCanExecuteChanged(); } }
        public String Unit { get { return _unit; } set { _unit = value; this.AddParameter.RaiseCanExecuteChanged(); } }

        public IDelegateCommand AddParameter
        {
            get
            {
                if (_addParameter == null)
                {
                    _addParameter = new DelegateCommand(OpenDialog, CanExecuteAdd);
                }
                return _addParameter;
            }
        }

        #endregion
        #region Конструкторы
        public AddParameterVM()
        {
            Name = String.Empty;
            Default = String.Empty;
            Unit = String.Empty;
            var sdbContext = SingletonDBContext.GetInstance(new SQLiteContext());
            _dbContext = sdbContext.DBContext;            
        }
        #endregion
        #region Методы
        private void ExecuteAdd(object parameter)
        {
            Parameter param = new Parameter()
            {
                Name = this.Name,
                Default = this.Default,
                Unit = this.Unit
            };

            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Parameter.Add(param);
                context.SaveChanges();
                context.Configuration.LazyLoadingEnabled = true;
                context.Database.Initialize(true);
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
            var openDialog = new DialogVM("Новый параметр", "Вы действительно хотите добавить новый параметр?", ExecuteAdd);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        #endregion
    }
}
