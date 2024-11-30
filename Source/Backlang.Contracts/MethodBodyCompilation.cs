namespace Backlang.Contracts;

public readonly record struct MethodBodyCompilation(
    LNode Function,
    CompilerContext Context,
    DescribedBodyMethod Method,
    QualifiedName? Modulename,
    Scope Scope);