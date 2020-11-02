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
    public class EditSpareVM : BaseVM
    {
        #region Поля
        private String _name, _description;
        private DbContext _dbContext;
        private Spare _spare;

        private IDelegateCommand _editSpare;
        #endregion
        #region Свойства
        public String Name { get { return _name; } set { _name = value; this.EditSpare.RaiseCanExecuteChanged(); } }
        public String Description { get { return _description; } set { _description = value; this.EditSpare.RaiseCanExecuteChanged(); } }
        public IDelegateCommand EditSpare
        {
            get
            {
                if (_editSpare == null)
                {
                    _editSpare = new DelegateCommand(OpenDialog, CanExecuteEdit);
                }
                return _editSpare;
            }
        }
        #endregion
        #region Конструкторы
        public EditSpareVM(long rowid)
        {
            Name = String.Empty;
            Description = String.Empty;
            var sdbContext = SingletonDBContext.GetInstance(new SQLiteContext());
            _dbContext = sdbContext.DBContext;
            if (_dbContext is SQLiteContext)
            {
                var context = _dbContext as SQLiteContext;
                _spare = context.Spare.Where(s => s.Rowid == rowid).SingleOrDefault();
                Name = _spare.Name;
                Description = _spare.Description;
            }
        }
        #endregion
        #region Методы
        private void ExecuteEdit(object parameter)
        {
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                Spare spare = context.Spare.Where(s => s.Rowid == _spare.Rowid).SingleOrDefault();
                spare.Name = Name;
                spare.Description = Description;
                context.SaveChanges();
                context.Configuration.LazyLoadingEnabled = true;
                context.Database.Initialize(true);
            }
        }

        private bool CanExecuteEdit(object parameter)
        {
            if ((Name != String.Empty) && (Description != String.Empty))
                return true;
            return false;
        }

        private async void OpenDialog(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM(String.Format("Изменить запчасть {0}", _spare.Name), "Вы действительно хотите изменить выбранную запчасть?", ExecuteEdit);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        #endregion
    }
}
