using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace CodegenAnalysis;

public static class ExpressionUtils
{
    public static (MethodInfo MethodInfo, object?[] Arguments) LambdaToMethodInfo(Expression<Action> expr)
    {
        if (expr.Body.NodeType != ExpressionType.Call)
            throw new ArgumentException("Expected a single call, for example, () => Function(5, 'a', 10)");
        var call = (MethodCallExpression)expr.Body;
        var mi = call.Method;
        if (call.Object is not null)
            throw new ArgumentException("Expected a static call");
        var args = new List<object?>(call.Arguments.Count);
        foreach (var arg in call.Arguments)
        {
            if (arg.NodeType == ExpressionType.Constant)
            {
                var c = (ConstantExpression)arg;
                args.Add(c.Value);
            }
            else
            {
                var evaluated = Expression.Lambda(arg).Compile().DynamicInvoke();
                args.Add(evaluated);
            }
        }
        return (mi, args.ToArray());
    }
}
