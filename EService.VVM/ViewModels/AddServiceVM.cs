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
    public class AddServiceVM : BaseVM
    {
        #region Поля
        private String _shortName, _fullName, _description;
        private long _price;
        private DbContext _dbContext;

        private IDelegateCommand _addService;
        #endregion
        #region Свойства
        public String ShortName { get { return _shortName; } set { _shortName = value; this.AddService.RaiseCanExecuteChanged(); } }
        public String FullName { get { return _fullName; } set { _fullName = value; this.AddService.RaiseCanExecuteChanged(); } }
        public String Description { get { return _description; } set { _description = value; this.AddService.RaiseCanExecuteChanged(); } }
        public long Price { get { return _price; } set { _price = value; this.AddService.RaiseCanExecuteChanged(); } }
        public IDelegateCommand AddService
        {
            get
            {
                if (_addService == null)
                {
                    _addService = new DelegateCommand(OpenDialog, CanExecuteAdd);
                }
                return _addService;
            }
        }

        #endregion
        #region Конструкторы
        public AddServiceVM()
        {
            ShortName = String.Empty;
            FullName = String.Empty;
            Description = String.Empty;
            var sdbContext = SingletonDBContext.GetInstance(new SQLiteContext());
            _dbContext = sdbContext.DBContext;            
        }
        #endregion
        #region Методы
        private void ExecuteAdd(object parameter)
        {
            Service service = new Service()
            {
                ShortName = this.ShortName,
                FullName = this.FullName,
                Description = this.Description,
                Price = this.Price
            };

            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Service.Add(service);
                context.SaveChanges();
            }
        }

        private bool CanExecuteAdd(object parameter)
        {
            if ((ShortName != String.Empty) && (FullName != String.Empty) && (Description != String.Empty))
                return true;
            return false;
        }

        private async void OpenDialog(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM("Новый сервис", "Вы действительно хотите добавить новый сервис?", ExecuteAdd);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        #endregion
    }
}
