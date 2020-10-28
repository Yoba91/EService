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
    public class EditModelVM : BaseVM
    {
        #region Поля
        private String _fullName, _shortName;
        private TypeModel _selectedTypeModel;
        private DbContext _dbContext;
        private Model _model;

        private IDelegateCommand _editModel;
        #endregion
        #region Свойства
        public String FullName { get { return _fullName; } set { _fullName = value; this.EditModel.RaiseCanExecuteChanged(); } }
        public String ShortName { get { return _shortName; } set { _shortName = value; this.EditModel.RaiseCanExecuteChanged(); } }
        public TypeModel TypeModel { get { return _selectedTypeModel; } set { _selectedTypeModel = value; this.EditModel.RaiseCanExecuteChanged(); } }
        public IList<TypeModel> TypesModel { get; set; }

        public IDelegateCommand EditModel
        {
            get
            {
                if (_editModel == null)
                {
                    _editModel = new DelegateCommand(OpenDialog, CanExecuteEdit);
                }
                return _editModel;
            }
        }
        #endregion
        #region Конструкторы
        public EditModelVM(long rowid)
        {
            FullName = String.Empty;
            ShortName = String.Empty;
            TypeModel = null;
            var sdbContext = SingletonDBContext.GetInstance(new SQLiteContext());
            _dbContext = sdbContext.DBContext;
            if (_dbContext is SQLiteContext)
            {
                var context = _dbContext as SQLiteContext;
                context.TypeModel.Load();
                TypesModel = context.TypeModel.Local.ToBindingList();
                _model = context.Model.Where(m => m.Rowid == rowid).SingleOrDefault();
                FullName = _model.FullName;
                ShortName = _model.ShortName;
                TypeModel = _model.TypeModel;
            }            
        }
        #endregion
        #region Методы
        private void ExecuteEdit(object parameter)
        {
            Model model = new Model()
            {
                FullName = this._fullName,
                ShortName = this._shortName,
                TypeModel = this._selectedTypeModel
            };

            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                Model editModel = context.Model.Where(s => s.Rowid == _model.Rowid).SingleOrDefault();
                editModel.FullName = FullName;
                editModel.ShortName = ShortName;
                editModel.TypeModel = TypeModel;
                context.SaveChanges();
            }
        }

        private bool CanExecuteEdit(object parameter)
        {
            if ((FullName != String.Empty) && (ShortName != String.Empty) && (TypeModel != null))
                return true;
            return false;
        }

        private async void OpenDialog(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM(String.Format("Изменить модель устройств {0}({1})", _model.FullName, _model.ShortName), "Вы действительно хотите изменить выбранную модель устройств?", ExecuteEdit);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        #endregion
    }
}
