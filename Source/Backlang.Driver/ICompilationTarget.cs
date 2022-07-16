using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Pipeline;

namespace Backlang.Driver;

public interface ICompilationTarget : ITarget
{
    TypeEnvironment Init(TypeResolver binder);

    void AfterCompiling(CompilerContext context);

    void BeforeCompiling(CompilerContext context);
}