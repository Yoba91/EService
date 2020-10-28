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
    public class EditTypeModelVM : BaseVM
    {
        #region Поля
        private String _fullName, _shortName;
        private TypeModel _typeModel;
        private DbContext _dbContext;

        private IDelegateCommand _editTypeModel;
        #endregion
        #region Свойства
        public String FullName { get { return _fullName; } set { _fullName = value; this.EditTypeModel.RaiseCanExecuteChanged(); } }
        public String ShortName { get { return _shortName; } set { _shortName = value; this.EditTypeModel.RaiseCanExecuteChanged(); } }

        public IDelegateCommand EditTypeModel
        {
            get
            {
                if (_editTypeModel == null)
                {
                    _editTypeModel = new DelegateCommand(OpenDialog, CanExecuteEdit);
                }
                return _editTypeModel;
            }
        }

        #endregion
        #region Конструкторы
        public EditTypeModelVM(long rowid)
        {
            FullName = String.Empty;
            ShortName = String.Empty;
            var sdbContext = SingletonDBContext.GetInstance(new SQLiteContext());
            _dbContext = sdbContext.DBContext;
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                _typeModel = context.TypeModel.Where(t => t.Rowid == rowid).SingleOrDefault();
                FullName = _typeModel.FullName;
                ShortName = _typeModel.ShortName;
            }
        }
        #endregion
        #region Методы
        private void ExecuteEdit(object parameter)
        {
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                TypeModel editTypeModel = context.TypeModel.Where(t => t.Rowid == _typeModel.Rowid).SingleOrDefault();
                editTypeModel.FullName = FullName;
                editTypeModel.ShortName = ShortName;
                context.SaveChanges();
            }
        }

        private bool CanExecuteEdit(object parameter)
        {
            if ((FullName != String.Empty) && (ShortName != String.Empty))
                return true;
            return false;
        }

        private async void OpenDialog(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM(String.Format("Изменить тип {0}", _typeModel.FullName), "Вы действительно хотите изменить выбранный тип?", ExecuteEdit);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        #endregion
    }
}
