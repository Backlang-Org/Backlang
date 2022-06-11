using Furesoft.Core.CodeDom.Compiler.Pipeline;

namespace Backlang.Driver.Compiling.Targets.Dotnet;

public class DotNetTarget : ITarget
{
    public string Name => "dotnet";

    public ITargetAssembly Compile(AssemblyContentDescription contents)
    {
        return new DotNetAssembly(contents);
    }
}