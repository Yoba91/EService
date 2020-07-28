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
        public ParameterExpression Parameter { get { return param; } set { param = value; } }
        public String SearchString { get { return search; } set { search = value?.ToUpper(); } }
        public FilterContains(ParameterExpression parameter)
        {
            Parameter = parameter;
            search = String.Empty;
        }
        public FilterContains(ParameterExpression parameter, string search)
        {
            Parameter = parameter;
            this.search = search.ToUpper();
        }
        public Expression CreateFilter(params string[] path)
        {
            MemberExpression mex = null;
            foreach (var item in path)
            {
                if (mex == null)
                { mex = Expression.PropertyOrField(param, item); }
                else
                { mex = Expression.PropertyOrField(mex, item); }
            }
            var condition = Expression.Call(mex, mex.Type.GetMethod("Contains"), Expression.Constant(SearchString));
            return condition;
        }
        static public Expression And(IList<Expression> expressions)
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

        static public Expression Or(IList<Expression> expressions)
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

        static public LambdaExpression GetLambda(Expression expression, ParameterExpression param)
        {
            return Expression.Lambda<Func<T, bool>>(expression, param);
        }
    }

    public class FilterDate<T> : IFilterData
    {
        DateTime start, end;
        ParameterExpression param = null;
        public ParameterExpression Parameter { get { return param; } set { param = value; } }
        public DateTime Start { get { return start; } set { start = value; } }
        public DateTime End { get { return end; } set { end = value; } }
        public FilterDate(ParameterExpression parameter)
        {
            Parameter = parameter;
            this.start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            this.end = DateTime.Now;
        }
        public FilterDate(ParameterExpression parameter, DateTime start, DateTime end)
        {
            Parameter = parameter;
            this.start = start;
            this.end = end;
        }
        public Expression CreateFilter(params string[] path)
        {
            MemberExpression mEx = null;
            foreach (var item in path)
            {
                if (mEx == null)
                { mEx = Expression.PropertyOrField(param, item); }
                else
                { mEx = Expression.PropertyOrField(mEx, item); }
            }
            var startExpr = Expression.Constant(start);
            var endExpr = Expression.Constant(end);
            var firstCondition = Expression.GreaterThanOrEqual(mEx, startExpr);
            var secondCondition = Expression.LessThanOrEqual(mEx, endExpr);
            var condition = Expression.AndAlso(firstCondition, secondCondition);
            return condition;
        }
        static public Expression And(IList<Expression> expressions)
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

        static public Expression Or(IList<Expression> expressions)
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

        static public LambdaExpression GetLambda(Expression expression, ParameterExpression param)
        {
            return Expression.Lambda<Func<T, bool>>(expression, param);
        }
    }


    public class FilterData<T> : IFilterData
    {
        long rowid;
        ParameterExpression param = null;
        public ParameterExpression Parameter { get { return param; } set { param = value; } }
        public long RowId { get { return rowid; } set { rowid = value; } }
        public FilterData(ParameterExpression parameter)
        {
            Parameter = parameter;
            this.RowId = 0;
        }
        public FilterData(ParameterExpression parameter, int rowid)
        {
            Parameter = parameter;
            this.RowId = rowid;
        }
        public Expression CreateFilter(params string[] path)
        {
            MemberExpression mEx = null;
            foreach (var item in path)
            {
                if (mEx == null)
                { mEx = Expression.PropertyOrField(param, item); }
                else
                { mEx = Expression.PropertyOrField(mEx, item); }
            }
            var exp = Expression.Constant(rowid);
            var condition = Expression.Equal(mEx, exp);
            return condition;
        }
        static public Expression And(IList<Expression> expressions)
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

        static public Expression Or(IList<Expression> expressions)
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

        static public LambdaExpression GetLambda(Expression expression, ParameterExpression param)
        {
            return Expression.Lambda<Func<T, bool>>(expression, param);
        }
    }
}
