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
    public class EditServiceVM : BaseVM
    {
        #region Поля
        private String _shortName, _fullName, _description;
        private long _price;
        private DbContext _dbContext;
        private Service _service;

        private IDelegateCommand _editService;
        #endregion
        #region Свойства
        public String ShortName { get { return _shortName; } set { _shortName = value; this.EditService.RaiseCanExecuteChanged(); } }
        public String FullName { get { return _fullName; } set { _fullName = value; this.EditService.RaiseCanExecuteChanged(); } }
        public String Description { get { return _description; } set { _description = value; this.EditService.RaiseCanExecuteChanged(); } }
        public long Price { get { return _price; } set { _price = value; this.EditService.RaiseCanExecuteChanged(); } }
        public IDelegateCommand EditService
        {
            get
            {
                if (_editService == null)
                {
                    _editService = new DelegateCommand(OpenDialog, CanExecuteEdit);
                }
                return _editService;
            }
        }
        #endregion
        #region Конструкторы
        public EditServiceVM(long rowid)
        {
            ShortName = String.Empty;
            FullName = String.Empty;
            Description = String.Empty;
            var sdbContext = SingletonDBContext.GetInstance(new SQLiteContext());
            _dbContext = sdbContext.DBContext;
            if (_dbContext is SQLiteContext)
            {
                var context = _dbContext as SQLiteContext;                
                _service = context.Service.Where(s => s.Rowid == rowid).SingleOrDefault();
                ShortName = _service.ShortName;
                FullName = _service.FullName;
                Description = _service.Description;
                Price = _service.Price;
            }
        }
        #endregion
        #region Методы
        private void ExecuteEdit(object parameter)
        {
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                Service service = context.Service.Where(s => s.Rowid == _service.Rowid).SingleOrDefault();
                service.ShortName = ShortName;
                service.FullName = FullName;
                service.Description = Description;
                service.Price = Price;
                context.SaveChanges();
            }
        }

        private bool CanExecuteEdit(object parameter)
        {
            if ((ShortName != String.Empty) && (FullName != String.Empty) && (Description != String.Empty))
                return true;
            return false;
        }

        private async void OpenDialog(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM(String.Format("Изменить сервис {0}({1})", _service.FullName, _service.ShortName), "Вы действительно хотите изменить выбранный сервис?", ExecuteEdit);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        #endregion
    }
}
