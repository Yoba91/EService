using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EService.BL
{
    public class FilterContains<T> : IFilterData
    {
        string search;

        ParameterExpression param = null;
        public String Search { get { return search; } set { search = value.ToUpper(); } }
        public FilterContains()
        {
            search = String.Empty;
        }
        public FilterContains(string search)
        {
            this.search = search.ToUpper();
        }
        public Expression CreateFilter(params string[] path)
        {
            if(param == null)
            param = Expression.Parameter(typeof(T), "s");
            MemberExpression mex = null;
            foreach (var item in path)
            {
                if (mex == null)
                { mex = Expression.PropertyOrField(param, item); }
                else
                { mex = Expression.PropertyOrField(mex, item); }
            }
            var condition = Expression.Call(mex, mex.Type.GetMethod("Contains"), Expression.Constant(Search));
            return condition;
        }
        public Expression And(IList<Expression> expressions)
        {
            Expression result = null;
            foreach (var item in expressions)
            {
                if (result == null)
                {
                    result = item;
                }
                else
                    result = Expression.And(result, item);
            }
            return result;
        }

        public Expression Or(IList<Expression> expressions)
        {
            Expression result = null;
            foreach (var item in expressions)
            {
                if (result == null)
                {
                    result = item;
                }
                else
                    result = Expression.Or(result, item);
            }
            return result;
        }

        public LambdaExpression GetLambda(Expression expression)
        {
            return Expression.Lambda<Func<T, bool>>(expression, param);
        }
    }

    public class FilterBuilder
    {
        
    }
}
