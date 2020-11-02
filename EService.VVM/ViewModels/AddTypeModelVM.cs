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
    public class AddTypeModelVM : BaseVM
    {
        #region Поля
        private String _fullName, _shortName;
        private DbContext _dbContext;

        private IDelegateCommand _addTypeModel;
        #endregion
        #region Свойства
        public String FullName { get { return _fullName; } set { _fullName = value; this.AddTypeModel.RaiseCanExecuteChanged(); } }
        public String ShortName { get { return _shortName; } set { _shortName = value; this.AddTypeModel.RaiseCanExecuteChanged(); } }

        public IDelegateCommand AddTypeModel
        {
            get
            {
                if (_addTypeModel == null)
                {
                    _addTypeModel = new DelegateCommand(OpenDialog, CanExecuteAdd);
                }
                return _addTypeModel;
            }
        }

        #endregion
        #region Конструкторы
        public AddTypeModelVM()
        {
            FullName = String.Empty;
            ShortName = String.Empty;
            var sdbContext = SingletonDBContext.GetInstance(new SQLiteContext());
            _dbContext = sdbContext.DBContext;
        }
        #endregion
        #region Методы
        private void ExecuteAdd(object parameter)
        {
            TypeModel typeModel = new TypeModel()
            {
                FullName = this.FullName,
                ShortName = this.ShortName
            };

            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.TypeModel.Add(typeModel);
                context.SaveChanges();
                context.Configuration.LazyLoadingEnabled = true;
                context.Database.Initialize(true);
            }
        }

        private bool CanExecuteAdd(object parameter)
        {
            if ((FullName != String.Empty) && (ShortName != String.Empty))
                return true;
            return false;
        }

        private async void OpenDialog(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM("Новый тип", "Вы действительно хотите добавить новый тип?", ExecuteAdd);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        #endregion
    }
}
