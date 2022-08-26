namespace Backlang.Contracts;

public interface ICompilationTarget : ITarget
{
    bool HasIntrinsics => IntrinsicType != null;
    Type IntrinsicType { get; }

    TypeEnvironment Init(CompilerContext context) => new DefaultTypeEnvironment();

    void InitReferences(CompilerContext context);

    void AfterCompiling(CompilerContext context);

    void BeforeCompiling(CompilerContext context);

    void BeforeExpandMacros(MacroProcessor processor);
}