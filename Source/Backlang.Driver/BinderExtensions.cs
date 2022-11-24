using System.Collections.Concurrent;

namespace Backlang.Driver;

public static class BinderExtensions
{
    private static readonly ConcurrentDictionary<string, IMethod> _functionCache = new();

    /// <summary>
    /// Finds a method based on a selector
    /// </summary>
    /// <param name="binder"></param>
    /// <param name="selector">System.StringBuilder::AppendLine(System.String)</param>
    /// <returns></returns>
    public static IMethod FindFunction(this TypeResolver binder, string selector)
    {
        if (_functionCache.ContainsKey(selector))
        {
            return _functionCache[selector];
        }

        var convertedSelector = GetSelector(selector);

        var type = binder.ResolveTypes(convertedSelector.Typename)?.FirstOrDefault();
        var methods = type.Methods
            .Where(_ => _.Name.ToString() == convertedSelector.FunctionName)
            .Where(_ => _.Parameters.Count == convertedSelector.ParameterTypes.Length);

        foreach (var method in methods)
        {
            for (int i = 0; i < method.Parameters.Count; i++)
            {
                if (method.Parameters[i].Type.FullName.ToString() == convertedSelector.ParameterTypes[i])
                {
                    _functionCache.AddOrUpdate(selector, _ => method, (_, __) => __);
                    return method;
                }
            }
        }

        var function = methods.FirstOrDefault();
        if (function != null)
        {
            _functionCache.AddOrUpdate(selector, _ => methods.FirstOrDefault(), (_, __) => __);
        }

        return methods.FirstOrDefault();
    }

    private static FunctionSelector GetSelector(string selector)
    {
        var ms = new FunctionSelector();

        var spl = selector.Split("::");

        ms.Typename = Utils.QualifyNamespace(spl[0]);

        var methodPart = spl[1];
        ms.FunctionName = methodPart.Substring(0, methodPart.IndexOf("("));

        var parameterPart = methodPart[ms.FunctionName.Length..].Trim('(', ')');
        ms.ParameterTypes = parameterPart
            .Split(",", StringSplitOptions.RemoveEmptyEntries)
            .ToArray();

        return ms;
    }

    private class FunctionSelector
    {
        public QualifiedName Typename { get; set; }
        public string FunctionName { get; set; }
        public string[] ParameterTypes { get; set; }
    }
}