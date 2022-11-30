namespace Backlang.Driver.Core.Implementors;

public struct StatementParameters
{
    public CompilerContext context;
    public IMethod method;
    public BasicBlockBuilder block;
    public LNode node;
    public QualifiedName? modulename;
    public Scope scope;

    public StatementParameters(CompilerContext context, IMethod method, BasicBlockBuilder block,
        LNode node, QualifiedName? modulename, Scope scope)
    {
        this.context = context;
        this.method = method;
        this.block = block;
        this.node = node;
        this.modulename = modulename;
        this.scope = scope;
    }
}