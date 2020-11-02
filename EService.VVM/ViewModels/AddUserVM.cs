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
    public class AddUserVM : BaseVM
    {
        #region Поля
        private String _name, _surname, _midname;
        private DbContext _dbContext;

        private IDelegateCommand _addUser;
        #endregion
        #region Свойства
        public String Name { get { return _name; } set { _name = value; this.AddUser.RaiseCanExecuteChanged(); } }
        public String Surname { get { return _surname; } set { _surname = value; this.AddUser.RaiseCanExecuteChanged(); } }
        public String Midname { get { return _midname; } set { _midname = value; this.AddUser.RaiseCanExecuteChanged(); } }
        public IDelegateCommand AddUser
        {
            get
            {
                if (_addUser == null)
                {
                    _addUser = new DelegateCommand(OpenDialog, CanExecuteAdd);
                }
                return _addUser;
            }
        }

        #endregion
        #region Конструкторы
        public AddUserVM()
        {
            Name = String.Empty;
            Surname = String.Empty;
            Midname = String.Empty;
            var sdbContext = SingletonDBContext.GetInstance(new SQLiteContext());
            _dbContext = sdbContext.DBContext;
        }
        #endregion
        #region Методы
        private void ExecuteAdd(object parameter)
        {
            Repairer user = new Repairer()
            {
                Name = this.Name,
                Surname = this.Surname,
                Midname = this.Midname,
                Password = "1111"
            };

            if (_dbContext is SQLiteContext)
            {
                SQLiteContext context = _dbContext as SQLiteContext;
                context.Repairer.Add(user);
                context.SaveChanges();
                context.Configuration.LazyLoadingEnabled = true;
                context.Database.Initialize(true);
            }
        }

        private bool CanExecuteAdd(object parameter)
        {
            if ((Name != String.Empty) && (Surname != String.Empty) && (Midname != String.Empty))
                return true;
            return false;
        }

        private async void OpenDialog(object parameter)
        {
            var displayRootRegistry = (Application.Current as App).displayRootRegistry;
            var openDialog = new DialogVM("Новый пользователь", "Вы действительно хотите добавить нового пользователя?", ExecuteAdd);
            await displayRootRegistry.ShowModalPresentation(openDialog);
        }
        #endregion
    }
}
