using EService.BL;
using EService.Data.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EService.VVM.ViewModels
{
    public class AddDeptVM : BaseVM
    {
        #region Поля
        private String _name, _code, _description;
        private DbContext _dbContext;

        private IDelegateCommand _addDept;
        #endregion
        #region Свойства
        public String Name { get { return _name; } set { _name = value; this.AddDept.RaiseCanExecuteChanged(); } }
        public String Code { get { return _code; } set { _code = value; this.AddDept.RaiseCanExecuteChanged(); } }
        public String Description { get { return _description; } set { _description = value; this.AddDept.RaiseCanExecuteChanged(); } }

        public IDelegateCommand AddDept
        {
            get
            {
                if (_addDept == null)
                {
                    _addDept = new DelegateCommand(OpenDialog, CanExecuteAdd);
                }
                return _addDept;
            }
        }

        #endregion
        #region Конструкторы
        public AddDeptVM()
        {
            Name = String.Empty;
            Code = String.Empty;
            Description = String.Empty;
            var sdbContext = SingletonDBContext.GetInstance(new SQLiteContext());
            _dbContext = sdbContext.DBContext;
        }
        #endregion
        #region Методы
        private void ExecuteAdd(object parameter)
        {
            Dept dept = new Dept()
            {
                Name = this.Name,
                Code = this.Code,
                Description = this.Description
            };

            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Dept.Add(dept);
                context.SaveChanges();
                context.Configuration.LazyLoadingEnabled = true;
                context.Database.Initialize(true);
            }
        }

        private bool CanExecuteAdd(object parameter)
        {
            if ((Name != String.Empty) && (Code != String.Empty) && (Description != String.Empty))
                return true;
            return false;
        }

        private async void OpenDialog(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM("Новый отдел", "Вы действительно хотите добавить новый отдел?", ExecuteAdd);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        #endregion
    }
}
