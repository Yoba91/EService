using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EService.BL
{
    public interface IFilter
    {
        void SetWhat(params string[] parameters);
        void SetWhere(params string[] parameters);
        Expression GetFilter();
        void CreateFilter();

        event Action FilterCreated;
    }
}
