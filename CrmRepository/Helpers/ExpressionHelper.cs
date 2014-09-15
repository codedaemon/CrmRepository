using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CrmRepository.Helpers
{
    public class ExpressionHelper
    {
        public static string GetPropertyNameFromExpression<T, TResult>(Expression<Func<T, TResult>> property)
        {
            return (GetMemberExpression(property).Member as PropertyInfo).Name;
        }

        private static MemberExpression GetMemberExpression<T, TResult>(Expression<Func<T, TResult>> expr)
        {
            var member = expr.Body as MemberExpression;
            var unary = expr.Body as UnaryExpression;
            return member ?? (unary != null ? unary.Operand as MemberExpression : null);
        }
    }
}
