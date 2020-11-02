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
    public class AddStatusVM : BaseVM
    {
        #region Поля
        private String _name;
        private DbContext _dbContext;

        private IDelegateCommand _addStatus;
        #endregion
        #region Свойства
        public String Name { get { return _name; } set { _name = value; this.AddStatus.RaiseCanExecuteChanged(); } }
        public IDelegateCommand AddStatus
        {
            get
            {
                if (_addStatus == null)
                {
                    _addStatus = new DelegateCommand(OpenDialog, CanExecuteAdd);
                }
                return _addStatus;
            }
        }

        #endregion
        #region Конструкторы
        public AddStatusVM()
        {
            Name = String.Empty;            
            var sdbContext = SingletonDBContext.GetInstance(new SQLiteContext());
            _dbContext = sdbContext.DBContext;
        }
        #endregion
        #region Методы
        private void ExecuteAdd(object parameter)
        {
            Status status = new Status()
            {
                Name = this.Name
            };

            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Status.Add(status);
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
            var openDialog = new DialogVM("Новый статус", "Вы действительно хотите добавить новый статус?", ExecuteAdd);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        #endregion
    }
}
