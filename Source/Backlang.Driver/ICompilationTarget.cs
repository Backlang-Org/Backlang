using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Pipeline;
using LeMP;
using Loyc.Syntax;

namespace Backlang.Driver;

public interface ICompilationTarget : ITarget
{
    bool HasIntrinsics { get; }
    Type IntrinsicType { get; }

    TypeEnvironment Init(TypeResolver binder);

    void AfterCompiling(CompilerContext context);

    void BeforeCompiling(CompilerContext context);

    void BeforeExpandMacros(MacroProcessor processor);

    LNode ConvertIntrinsic(LNode calls);
}