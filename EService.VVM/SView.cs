using EService.BL;
using EService.Data.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EService.VVM
{
    public class SView
    {
        private string _categories;
        public ServiceLog ServiceLog { get; private set; }
        public IList<ServiceCategory> ServiceCategories { get; private set; }
        public int Index { get; set; }
        public string Categories
        {
            get
            {
                return _categories;
            }
            set
            {
                StringBuilder str = new StringBuilder();
                ServiceCategories.ToList().ForEach(s => str.Append(s.Name + ","));
                _categories = str.ToString().TrimEnd(',');
            }
        }

        public SView(ServiceLog serviceLog, int index)
        {
            ServiceLog = serviceLog;
            ServiceCategories = CalculateCategory(serviceLog);
            Categories = String.Empty;
            Index = index;
        }
        private IList<ServiceCategory> CalculateCategory(ServiceLog serviceLog)
        {
            List<ServiceCategory> serviceCategories = new List<ServiceCategory>();
            long complexity = 0;
            serviceLog.ServicesDone.ToList().ForEach(i => complexity += i.ServiceForModel.Service.Price);
            DbContext dbContext = SingletonDBContext.GetInstance(new SQLiteContext()).DBContext;
            if (dbContext is SQLiteContext)
            {
                var context = dbContext as SQLiteContext;
                serviceCategories = context.ServiceCategory.ToList().FindAll(s => complexity >= s.MinValue && complexity <= s.MaxValue).ToList();
            }
            return serviceCategories;
        }
    }
}
