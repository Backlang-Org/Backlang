namespace Backlang.Contracts;

public interface ICompilationTarget : ITarget
{
    bool HasIntrinsics => IntrinsicType != null;
    Type IntrinsicType { get; }

    TypeEnvironment Init(CompilerContext context)
    {
        var te = new DefaultTypeEnvironment();
        context.Binder.AddAssembly(te.Assembly);

        return te;
    }

    void InitReferences(CompilerContext context);

    void AfterCompiling(CompilerContext context);

    void BeforeCompiling(CompilerContext context);

    void BeforeExpandMacros(MacroProcessor processor);
}