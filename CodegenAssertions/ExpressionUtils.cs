using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace CodegenAssertions;

internal static class ExpressionUtils
{
    internal static (MethodInfo MethodInfo, object?[] Arguments) LambdaToMethodInfo(Expression<Action> expr)
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
            if (arg.NodeType != ExpressionType.Constant)
                throw new ArgumentException($"Expected a constant, got {arg} instead");
            var c = (ConstantExpression)arg;
            args.Add(c.Value);
        }
        return (mi, args.ToArray());
    }
}
