using System;
using System.Linq.Expressions;
using System.Reflection;

namespace EasyNetQ
{
    internal static class ExtendsExpression
    {
        public static PropertyInfo GetPropertyInfoFromExpression<T>(this Expression<Func<T, object>> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            var lambda = value as LambdaExpression;
            if (lambda == null)
            {
                throw new ArgumentException("Not a lambda expression", "value");
            }
            MemberExpression memberExpr = null;
            if (lambda.Body.NodeType == ExpressionType.Convert)
            {
                memberExpr = ((UnaryExpression)lambda.Body).Operand as MemberExpression;
            }
            else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpr = lambda.Body as MemberExpression;
            }
            if (memberExpr == null)
            {
                throw new ArgumentException("Not a member access", "value");
            }

            return memberExpr.Member as PropertyInfo;
        }
    }
}