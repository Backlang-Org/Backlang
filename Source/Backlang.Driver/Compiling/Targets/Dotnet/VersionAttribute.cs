namespace Backlang.Driver.Compiling.Targets.Dotnet;

internal class VersionAttribute : IAttribute
{
    public Version Version { get; internal set; }

    public IType AttributeType => null;
}