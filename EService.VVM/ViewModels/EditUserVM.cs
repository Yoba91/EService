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
    public class EditUserVM : BaseVM
    {
        #region Поля
        private String _name, _surname, _midname;
        private DbContext _dbContext;
        private Repairer _user;

        private IDelegateCommand _editUser;
        #endregion
        #region Свойства
        public String Name { get { return _name; } set { _name = value; this.EditUser.RaiseCanExecuteChanged(); } }
        public String Surname { get { return _surname; } set { _surname = value; this.EditUser.RaiseCanExecuteChanged(); } }
        public String Midname { get { return _midname; } set { _midname = value; this.EditUser.RaiseCanExecuteChanged(); } }
        public IDelegateCommand EditUser
        {
            get
            {
                if (_editUser == null)
                {
                    _editUser = new DelegateCommand(OpenDialog, CanExecuteEdit);
                }
                return _editUser;
            }
        }
        #endregion
        #region Конструкторы
        public EditUserVM(long rowid)
        {
            Name = String.Empty;
            Surname = String.Empty;
            Midname = String.Empty;
            var sdbContext = SingletonDBContext.GetInstance(new SQLiteContext());
            _dbContext = sdbContext.DBContext;
            if (_dbContext is SQLiteContext)
            {
                var context = _dbContext as SQLiteContext;
                _user = context.Repairer.Where(r => r.Rowid == rowid).SingleOrDefault();
                Name = _user.Name;
                Surname = _user.Surname;
                Midname = _user.Midname;
            }
        }
        #endregion
        #region Методы
        private void ExecuteEdit(object parameter)
        {
            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                Repairer user = context.Repairer.Where(r => r.Rowid == _user.Rowid).SingleOrDefault();
                user.Name = Name;
                user.Surname = Surname;
                user.Midname = Midname;
                user.Password = "0000";
                context.SaveChanges();
            }
        }

        private bool CanExecuteEdit(object parameter)
        {
            if ((Name != String.Empty) && (Surname != String.Empty) && (Midname != String.Empty))
                return true;
            return false;
        }

        private async void OpenDialog(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM(String.Format("Изменить пользователя \"{0} {1} {2}\"", _user.Surname, _user.Name, _user.Midname), "Вы действительно хотите изменить выбранного пользователя?", ExecuteEdit);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        #endregion
    }
}
