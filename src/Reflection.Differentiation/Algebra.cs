using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Reflection.Differentiation
{
    public static class Algebra
    {
        public static Expression<Func<double, double>> Differentiate(Expression<Func<double, double>> f)
        {
            var parameter = f.Parameters[0];

            var differentiatedBody = Differentiate(f.Body, parameter.Name);

            return Expression.Lambda<Func<double, double>>(differentiatedBody, parameter);
        }

        private static Expression Differentiate(Expression f, string param)
        {
            switch (f.NodeType)
            {
                case ExpressionType.Constant:
                    return Expression.Constant(0d);

                case ExpressionType.Parameter:
                    return Expression.Constant(1d);

                case ExpressionType.Add:
                    var la = ((BinaryExpression)f).Left;
                    var ra = ((BinaryExpression)f).Right;

                    return Expression.Add(Differentiate(la, param), Differentiate(ra, param));

                case ExpressionType.Subtract:
                    var ls = ((BinaryExpression)f).Left;
                    var rs = ((BinaryExpression)f).Right;

                    return Expression.Subtract(Differentiate(ls, param), Differentiate(rs, param));

                case ExpressionType.Multiply:
                    var lm = ((BinaryExpression)f).Left;
                    var rm = ((BinaryExpression)f).Right;

                    if (lm.NodeType == ExpressionType.Constant)
                    {
                        return Expression.Multiply(lm, Differentiate(rm, param));
                    }

                    if (rm.NodeType == ExpressionType.Constant)
                    {
                        return Expression.Multiply(Differentiate(lm, param), rm);
                    }

                    var dlm = Differentiate(lm, param);
                    var drm = Differentiate(rm, param);

                    return Expression.Add(Expression.Multiply(dlm, rm), Expression.Multiply(lm, drm));

                case ExpressionType.Call:
                    return Expression.Multiply(
                        Differentiate(f as MethodCallExpression),
                        Differentiate((f as MethodCallExpression).Arguments[0], param)
                        );

                default:
                    throw new ArgumentException($"Unknown expression {f}");
            }

            return f;
        }

        private static Expression Differentiate(MethodCallExpression f)
        {
            var mi = f.Method;

            if (!mi.IsStatic || mi.DeclaringType.FullName != "System.Math")
            {
                throw new ArgumentException($"Unsupported function invocation {mi.Name} on type {mi.DeclaringType.FullName}");
            }

            switch (mi.Name)
            {
                case "Sin":
                    return Expression.Call(null, typeof(Math).GetMethod("Cos"), f.Arguments[0]);

                case "Cos":
                    return Expression.Negate(Expression.Call(null, typeof(Math).GetMethod("Sin"), f.Arguments[0]));

            }

            throw new ArgumentException($"Unsupported Math function invocation {mi.Name}");
        }
    }
}
