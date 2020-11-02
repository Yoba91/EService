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
    public class AddSpareVM : BaseVM
    {
        #region Поля
        private String _name, _description;
        private DbContext _dbContext;

        private IDelegateCommand _addSpare;
        #endregion
        #region Свойства
        public String Name { get { return _name; } set { _name = value; this.AddSpare.RaiseCanExecuteChanged(); } }
        public String Description { get { return _description; } set { _description = value; this.AddSpare.RaiseCanExecuteChanged(); } }
        public IDelegateCommand AddSpare
        {
            get
            {
                if (_addSpare == null)
                {
                    _addSpare = new DelegateCommand(OpenDialog, CanExecuteAdd);
                }
                return _addSpare;
            }
        }

        #endregion
        #region Конструкторы
        public AddSpareVM()
        {
            Name = String.Empty;
            Description = String.Empty;
            var sdbContext = SingletonDBContext.GetInstance(new SQLiteContext());
            _dbContext = sdbContext.DBContext;
        }
        #endregion
        #region Методы
        private void ExecuteAdd(object parameter)
        {
            Spare spare = new Spare()
            {
                Name = this.Name,
                Description = this.Description
            };

            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Spare.Add(spare);
                context.SaveChanges();
                context.Configuration.LazyLoadingEnabled = true;
                context.Database.Initialize(true);
            }
        }

        private bool CanExecuteAdd(object parameter)
        {
            if ((Name != String.Empty) && (Description != String.Empty))
                return true;
            return false;
        }

        private async void OpenDialog(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM("Новая запчасть", "Вы действительно хотите добавить новую запчасть?", ExecuteAdd);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        #endregion
    }
}
