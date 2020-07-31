using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EService.BL
{
    public class FilterSearch : Filter
    {
        private ParameterExpression parameter;

        private MemberExpression member = null;

        private List<MemberExpression> members = null;

        private string searchString;

        private Expression filter;

        public MemberExpression Member { get { return member; } }

        public FilterSearch(ParameterExpression parameter) : base(parameter)
        {
            Parameter = parameter;
            searchString = String.Empty;
            members = new List<MemberExpression>();
            filter = null;
        }

        public override ParameterExpression Parameter { get { return parameter; } set { parameter = value; } }

        public override event Action FilterCreated;

        public override void CreateFilter()
        {
            filter = null;
            if(members.Count > 0)
            {
                foreach (var item in members)
                {
                    if(filter == null)
                        filter = Expression.Call(item, item.Type.GetMethod("Contains"), Expression.Constant(searchString));
                    else
                        filter = Expression.Or(filter, Expression.Call(item, item.Type.GetMethod("Contains"), Expression.Constant(searchString)));
                }
            }
            else
            filter = Expression.Call(member, member.Type.GetMethod("Contains"), Expression.Constant(searchString));
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
