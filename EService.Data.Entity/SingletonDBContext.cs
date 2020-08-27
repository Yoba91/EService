using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EService.BL
{
    public class SingletonDBContext
    {
        private SingletonDBContext() { }

        private static SingletonDBContext _instance;

        private static readonly object _lock = new object();

        public static SingletonDBContext GetInstance(DbContext dbContext)
        {
            if(_instance == null)
            {
                lock(_lock)
                {
                    if(_instance == null)
                    {
                        _instance = new SingletonDBContext();
                        _instance.DBContext = dbContext;
                    }
                }
            }
            return _instance;
        }

        public DbContext DBContext { get; private set; }
    }
}
