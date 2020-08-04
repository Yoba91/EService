using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EService.BL
{
    public class FilterIdAnd : FilterId
    {
        public FilterIdAnd(ParameterExpression parameter) : base(parameter)
        {
            Parameter = parameter;
            numbers = new List<long>();
            var constant = Expression.Constant(0);
            filter = Expression.Equal(constant, constant);
        }

        public override ParameterExpression Parameter { get { return parameter; } set { parameter = value; } }

        public override event Action FilterCreated;

        public override void CreateFilter()
        {
            Expression constant = null, condition = null;
            filter = null;
            foreach (var item in numbers)
            {
                constant = Expression.Constant(item);
                condition = Expression.Equal(member, constant);
                if (filter != null)
                { filter = Expression.Or(filter, condition); }
                else
                { filter = condition; }
            }
            if(filter==null)
            {
                constant = Expression.Constant(0);
                filter = Expression.Equal(constant, constant);
            }
            FilterCreated?.Invoke();
        }

        public override Expression GetFilter()
        {
            return filter;
        }

        public override void SetWhat(params string[] parameters)
        {
            numbers.Clear();
            if (parameters?.Count<string>() > 0)
            {
                foreach (var item in parameters)
                {
                    numbers.Add(long.Parse(item));
                }
            }
        }

        public override void SetWhere(params string[] parameters)
        {
            member = null;
            foreach (var item in parameters)
            {
                if (member == null)
                { member = Expression.PropertyOrField(parameter, item); }
                else
                { member = Expression.PropertyOrField(member, item); }
            }
        }
    }
}
