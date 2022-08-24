namespace Backlang.Contracts;

public interface ICompilationTarget : ITarget
{
    bool HasIntrinsics { get; }
    Type IntrinsicType { get; }

    TypeEnvironment Init(CompilerContext context);

    void InitReferences(CompilerContext context);

    void AfterCompiling(CompilerContext context);

    void BeforeCompiling(CompilerContext context);

    void BeforeExpandMacros(MacroProcessor processor);
}