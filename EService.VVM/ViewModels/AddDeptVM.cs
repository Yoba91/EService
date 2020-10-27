using EService.BL;
using EService.Data.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EService.VVM.ViewModels
{
    public class AddDeptVM : INotifyPropertyChanged
    {
        #region Поля
        private DbContext dbContext;

        private IDelegateCommand addDept;
        #endregion
        #region Свойства
        public String Name { get; set; }
        public String Code { get; set; }
        public String Description { get; set; }
        #endregion
        #region Конструкторы
        public AddDeptVM()
        {
            var sdbContext = SingletonDBContext.GetInstance(new SQLiteContext());
            dbContext = sdbContext.DBContext;
        }
        #endregion
        #region Методы
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        #endregion
    }
}
