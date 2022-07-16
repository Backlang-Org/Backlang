using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Pipeline;

namespace Backlang.Driver.Compiling.Targets.bs2k;

public class BS2KTarget : ICompilationTarget
{
    public string Name => "bs2k";

    public void AfterCompiling(CompilerContext context)
    {
    }

    public ITargetAssembly Compile(AssemblyContentDescription contents)
    {
        return new Bs2kAssembly(contents);
    }

    public TypeEnvironment Init(TypeResolver binder)
    {
        var te = new Bs2KTypeEnvironment();

        binder.AddAssembly(te.Assembly);

        return te;
    }
}