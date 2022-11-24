namespace Backlang.Driver.Core;

public static class GenericTypeMap
{
    public static readonly Dictionary<(QualifiedName, IMember), object> Cache = new();
}