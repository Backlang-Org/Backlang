using Backlang.Contracts;
using Furesoft.Core.CodeDom.Compiler.Pipeline;
using LeMP;
using System.ComponentModel.Composition;

namespace Backlang.Backends.Bs2k;

[Export(typeof(ICompilationTarget))]
public class BS2KTarget : ICompilationTarget
{
    public string Name => "bs2k";

    public Type IntrinsicType => typeof(Intrinsics);

    public void AfterCompiling(CompilerContext context)
    {
    }

    public void BeforeCompiling(CompilerContext context)
    {
        context.Options.OutputFilename += ".bsm";
    }

    public void BeforeExpandMacros(MacroProcessor processor)
    {
    }

    public ITargetAssembly Compile(AssemblyContentDescription contents)
    {
        return new Bs2kAssembly(contents);
    }

    public void InitReferences(CompilerContext context)
    {
    }
}