using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using EService.Data.Entity;

namespace EService.Data
{
    public interface IDataHandler<T>
    {
        void SetDataType(T dataType);

        void DataRepositoryConnection();

        ICollection<IData> GetData(Type type, Expression filter);



    }
}
