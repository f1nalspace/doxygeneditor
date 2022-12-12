using System;
using System.Linq.Expressions;

namespace TSP.DoxygenEditor.Utils
{
    static class ReflectionUtils
    {
        public static string GetName(Expression<Func<object>> exp)
        {
            MemberExpression body = exp.Body as MemberExpression;
            if (body == null)
            {
                UnaryExpression ubody = (UnaryExpression)exp.Body;
                body = ubody.Operand as MemberExpression;
            }
            return body.Member.Name;
        }
    }
}
