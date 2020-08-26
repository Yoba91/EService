using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EService.BL
{
    public class FilterSearch : Filter
    {
        private ParameterExpression parameter;

        private MemberExpression member = null;

        private List<MemberExpression> members = null;

        StringComparison strComp;

        private string searchString;

        private Expression filter;

        public MemberExpression Member { get { return member; } }

        public String SearchString { get { return searchString; } }

        public List<MemberExpression> Members { get { return members; } }

        public FilterSearch(ParameterExpression parameter) : base(parameter)
        {
            Parameter = parameter;
            searchString = String.Empty;
            members = new List<MemberExpression>();
            filter = null;
            strComp = StringComparison.InvariantCultureIgnoreCase;
        }

        public override ParameterExpression Parameter { get { return parameter; } set { parameter = value; } }

        public override event Action FilterCreated;

        public override void CreateFilter()
        {
            filter = null;
            if (members.Count > 0)
            {
                foreach (var item in members)
                {
                    if (filter == null)
                    {
                        filter = Expression.Call(item, "IndexOf", null, Expression.Constant(searchString, typeof(string)), Expression.Constant(strComp));
                        filter = Expression.GreaterThanOrEqual(filter, Expression.Constant(0));
                    }
                    else
                    {
                        Expression tempfilter = Expression.Call(item, "IndexOf", null, Expression.Constant(searchString, typeof(string)), Expression.Constant(strComp));
                        tempfilter = Expression.GreaterThanOrEqual(tempfilter, Expression.Constant(0));
                        filter = Expression.Or(filter, tempfilter);
                    }
                }
            }
            else
            {
                filter = Expression.Call(member, "IndexOf", null, Expression.Constant(searchString, typeof(string)), Expression.Constant(strComp));
                filter = Expression.GreaterThanOrEqual(filter, Expression.Constant(0));
            }
            members.Clear();
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
                searchString = parameters[0];
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

        public virtual void AddWhere(MemberExpression member)
        {
            members.Add(member);
        }
    }
}
