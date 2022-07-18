using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Pipeline;
using LeMP;
using Loyc;
using Loyc.Syntax;

namespace Backlang.Driver.Compiling.Targets.bs2k;

public class BS2KTarget : ICompilationTarget
{
    public string Name => "bs2k";

    public bool HasIntrinsics => true;

    public Type IntrinsicType => typeof(Intrinsics);

    public void AfterCompiling(CompilerContext context)
    {
    }

    public void BeforeCompiling(CompilerContext context)
    {
        context.OutputFilename += ".bsm";
    }

    public void BeforeExpandMacros(MacroProcessor processor)
    {
    }

    public ITargetAssembly Compile(AssemblyContentDescription contents)
    {
        return new Bs2kAssembly(contents);
    }

    public LNode ConvertIntrinsic(LNode call)
    {
        return "Backlang".dot("Driver").dot("Compiling").dot("Targets").dot("bs2k").dot("Intrinsics").coloncolon(call).Target;
    }

    public TypeEnvironment Init(TypeResolver binder)
    {
        return new Bs2KTypeEnvironment();
    }
}