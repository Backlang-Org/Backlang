namespace Backlang.Driver.Core;

public static class GenericTypeMap
{
    public static Dictionary<(QualifiedName, IMember), object> Cache = new();
}