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
    public class EditStatusVM : BaseVM
    {
        #region Поля
        private String _name;
        private Status _status;
        private DbContext _dbContext;

        private IDelegateCommand _editStatus;
        #endregion
        #region Свойства
        public String Name { get { return _name; } set { _name = value; this.EditStatus.RaiseCanExecuteChanged(); } }

        public IDelegateCommand EditStatus
        {
            get
            {
                if (_editStatus == null)
                {
                    _editStatus = new DelegateCommand(OpenDialog, CanExecuteEdit);
                }
                return _editStatus;
            }
        }

        #endregion
        #region Конструкторы
        public EditStatusVM(long rowid)
        {
            Name = String.Empty;
            var sdbContext = SingletonDBContext.GetInstance(new SQLiteContext());
            _dbContext = sdbContext.DBContext;
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                _status = context.Status.Where(s => s.Rowid == rowid).SingleOrDefault();
                Name = _status.Name;
            }
        }
        #endregion
        #region Методы
        private void ExecuteEdit(object parameter)
        {
            Status status = new Status()
            {
                Name = this.Name,
            };

            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                Status editStatus = context.Status.Where(s => s.Rowid == _status.Rowid).SingleOrDefault();
                editStatus.Name = Name;
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
            var openDialog = new DialogVM(String.Format("Изменить статус {0}", _status.Name), "Вы действительно хотите изменить выбранный статус?", ExecuteEdit);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        #endregion
    }
}
