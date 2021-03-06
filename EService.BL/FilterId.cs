﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EService.BL
{
    public class FilterId : Filter
    {
        protected IList<long> numbers;

        protected MemberExpression member = null;

        protected Expression filter;

        protected ParameterExpression parameter;

        public FilterId(ParameterExpression parameter) : base(parameter)
        {
            Parameter = parameter;
            numbers = new List<long>();
            filter = null;
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
