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
        //ToDo: make namespace calls simpler with helper method qualifiyng from namespace string

        return LNode.Call(
                LNode.Call(LNode.Id("'::"),
                LNode.List(LNode.Call(CodeSymbols.Dot,
                LNode.List(LNode.Call(CodeSymbols.Dot,
                LNode.List(LNode.Call(CodeSymbols.Dot,
                LNode.List(LNode.Call(CodeSymbols.Dot,
                LNode.List(LNode.Call(CodeSymbols.Dot,
                LNode.List(LNode.Id((Symbol)"Backlang"),
                LNode.Id((Symbol)"Driver"))).SetStyle(NodeStyle.Operator),
                LNode.Id((Symbol)"Compiling"))).SetStyle(NodeStyle.Operator),
                LNode.Id((Symbol)"Targets"))).SetStyle(NodeStyle.Operator),
                LNode.Id((Symbol)"bs2k"))).SetStyle(NodeStyle.Operator),
                LNode.Id((Symbol)"Intrinsics"))).SetStyle(NodeStyle.Operator),
            call)).SetStyle(NodeStyle.Operator)).Target;
    }

    public TypeEnvironment Init(TypeResolver binder)
    {
        return new Bs2KTypeEnvironment();
    }
}