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
    public class EditParameterVM : BaseVM
    {
        #region Поля
        private String _name, _default, _unit;
        private DbContext _dbContext;
        private Parameter _parameter;

        private IDelegateCommand _editParameter;
        #endregion
        #region Свойства
        public String Name { get { return _name; } set { _name = value; this.EditParameter.RaiseCanExecuteChanged(); } }
        public String Default { get { return _default; } set { _default = value; this.EditParameter.RaiseCanExecuteChanged(); } }
        public String Unit { get { return _unit; } set { _unit = value; this.EditParameter.RaiseCanExecuteChanged(); } }
        public IDelegateCommand EditParameter
        {
            get
            {
                if (_editParameter == null)
                {
                    _editParameter = new DelegateCommand(OpenDialog, CanExecuteEdit);
                }
                return _editParameter;
            }
        }
        #endregion
        #region Конструкторы
        public EditParameterVM(long rowid)
        {
            Name = String.Empty;
            Default = String.Empty;
            Unit = String.Empty;
            var sdbContext = SingletonDBContext.GetInstance(new SQLiteContext());
            _dbContext = sdbContext.DBContext;
            if (_dbContext is SQLiteContext)
            {
                var context = _dbContext as SQLiteContext;                
                _parameter = context.Parameter.Where(p => p.Rowid == rowid).SingleOrDefault();
                Name = _parameter.Name;
                Default = _parameter.Default;
                Unit = _parameter.Unit;
            }
        }
        #endregion
        #region Методы
        private void ExecuteEdit(object parameter)
        {
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                Parameter param = context.Parameter.Where(p => p.Rowid == _parameter.Rowid).SingleOrDefault();
                param.Name = Name;
                param.Default = Default;
                param.Unit = Unit;
                context.SaveChanges();
                context.Configuration.LazyLoadingEnabled = true;
                context.Database.Initialize(true);
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
            var openDialog = new DialogVM(String.Format("Изменить параметр {0}", _parameter.Name), "Вы действительно хотите изменить выбранный параметр?", ExecuteEdit);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        #endregion
    }
}
