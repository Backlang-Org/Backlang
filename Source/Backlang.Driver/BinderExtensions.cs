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
    /// <returns>The found function or null</returns>
    public static IMethod FindFunction(this TypeResolver binder, string selector)
    {
        if (_functionCache.TryGetValue(selector, out var value))
        {
            return value;
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
        var result = new FunctionSelector();

        var splittedSelectorString = selector.Split("::");

        result.Typename = Utils.QualifyNamespace(splittedSelectorString[0]);

        var methodPart = splittedSelectorString[1];
        result.FunctionName = methodPart[..methodPart.IndexOf("(")];

        var parameterPart = methodPart[result.FunctionName.Length..].Trim('(', ')');
        result.ParameterTypes = parameterPart
            .Split(",", StringSplitOptions.RemoveEmptyEntries)
            .ToArray();

        return result;
    }

    private class FunctionSelector
    {
        public QualifiedName Typename { get; set; }
        public string FunctionName { get; set; }
        public string[] ParameterTypes { get; set; }
    }
}