using System;
using System.Linq.Expressions;

namespace DoxygenEditor.Utils
{
    static class ReflectionUtils
    {
        public static string GetMemberName<T, TValue>(Expression<Func<T, TValue>> memberAccess)
        {
            return ((MemberExpression)memberAccess.Body).Member.Name;
        }
    }
}
