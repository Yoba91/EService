using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EService.BL
{
    public class FilterDate : Filter
    {
        private ParameterExpression parameter;

        private MemberExpression member = null;

        private DateTime start, stop;

        private Expression filter;

        public FilterDate(ParameterExpression parameter) : base(parameter)
        {
            Parameter = parameter;
            start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            stop = DateTime.Now;
            filter = null;
        }

        public override ParameterExpression Parameter { get { return parameter; } set { parameter = value; } }

        public override event Action FilterCreated;

        public override void CreateFilter()
        {
            filter = null;
            var startExpr = Expression.Constant(start);
            var endExpr = Expression.Constant(stop);
            var firstCondition = Expression.GreaterThanOrEqual(member, startExpr);
            var secondCondition = Expression.LessThanOrEqual(member, endExpr);
            filter = Expression.AndAlso(firstCondition, secondCondition);
            FilterCreated?.Invoke();
        }

        public override Expression GetFilter()
        {
            return filter;
        }

        public override void SetWhat(params string[] parameters)
        {
            if (parameters?.Count<string>() > 0)
            {
                if (parameters.Count<string>() == 1)
                {
                    int year = 0, month = 0, day = 0;
                    try
                    {
                        year = int.Parse(parameters[0].Split('.')[2]);
                        month = int.Parse(parameters[0].Split('.')[1]);
                        day = int.Parse(parameters[0].Split('.')[0]);
                    }
                    catch (Exception e)
                    {
                        e.ToString();
                    }
                    start = new DateTime(year, month, day);
                    stop = new DateTime(year, month, day);
                }
                else
                {
                    int year = 0, month = 0, day = 0;
                    try
                    {
                        year = int.Parse(parameters[0].Split('.')[2]);
                        month = int.Parse(parameters[0].Split('.')[1]);
                        day = int.Parse(parameters[0].Split('.')[0]);
                    }
                    catch (Exception e)
                    {
                        e.ToString();
                    }
                    start = new DateTime(year, month, day);
                    try
                    {
                        year = int.Parse(parameters[1].Split('.')[2]);
                        month = int.Parse(parameters[1].Split('.')[1]);
                        day = int.Parse(parameters[1].Split('.')[0]);
                    }
                    catch (Exception e)
                    {
                        e.ToString();
                    }
                    stop = new DateTime(year, month, day);

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
