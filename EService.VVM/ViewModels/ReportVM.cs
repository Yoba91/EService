using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Excel = Microsoft.Office.Interop.Excel;
using EService.Data.Entity;

namespace EService.VVM.ViewModels
{
    public class ReportVM : BaseVM
    {
        #region Поля
        private IList<SView> _serviceLogs;
        #endregion
        #region Свойства
        public IList<SView> ServiceLogs { get { return _serviceLogs; } set { _serviceLogs = value; } }
        #endregion
        #region Кострукторы
        public ReportVM(IList<SView> serviceLogs)
        {

        }
        #endregion
        #region Методы
        #endregion
    }
}
