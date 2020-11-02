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
    public class EditDeptVM : BaseVM
    {
        #region Поля
        private String _name, _code, _description;
        private Dept _dept;
        private DbContext _dbContext;

        private IDelegateCommand _editDept;
        #endregion
        #region Свойства
        public String Name { get { return _name; } set { _name = value; this.EditDept.RaiseCanExecuteChanged(); } }
        public String Code { get { return _code; } set { _code = value; this.EditDept.RaiseCanExecuteChanged(); } }
        public String Description { get { return _description; } set { _description = value; this.EditDept.RaiseCanExecuteChanged(); } }

        public IDelegateCommand EditDept
        {
            get
            {
                if (_editDept == null)
                {
                    _editDept = new DelegateCommand(OpenDialog, CanExecuteEdit);
                }
                return _editDept;
            }
        }

        #endregion
        #region Конструкторы
        public EditDeptVM(long rowid)
        {
            Name = String.Empty;
            Code = String.Empty;
            Description = String.Empty;
            var sdbContext = SingletonDBContext.GetInstance(new SQLiteContext());
            _dbContext = sdbContext.DBContext;
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                _dept = context.Dept.Where(d => d.Rowid == rowid).SingleOrDefault();
                Name = _dept.Name;
                Code = _dept.Code;
                Description = _dept.Description;
            }
        }
        #endregion
        #region Методы
        private void ExecuteEdit(object parameter)
        {
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                Dept editDept = context.Dept.Where(d => d.Rowid == _dept.Rowid).SingleOrDefault();
                editDept.Name = Name;
                editDept.Code = Code;
                editDept.Description = Description;
                context.SaveChanges();
                context.Configuration.LazyLoadingEnabled = true;
                context.Database.Initialize(true);
            }
        }

        private bool CanExecuteEdit(object parameter)
        {
            if ((Name != String.Empty) && (Code != String.Empty) && (Description != String.Empty))
                return true;
            return false;
        }

        private async void OpenDialog(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM(String.Format("Изменить отдел {0}({1})",_dept.Name,_dept.Code), "Вы действительно хотите изменить выбранный отдел?", ExecuteEdit);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        #endregion
    }
}
