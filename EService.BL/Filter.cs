using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EService.BL
{
    public abstract class Filter : IFilter
    {
        public abstract ParameterExpression Parameter { get; set; }

        public abstract event Action FilterCreated;

        public Filter(ParameterExpression parameter)
        {
            Parameter = parameter;
        }

        public abstract void CreateFilter();

        public abstract Expression GetFilter();

        public abstract void SetWhat(params string[] parameters);

        public abstract void SetWhere(params string[] parameters);
    }
}
