namespace Backlang.Driver.Core.Implementors;

public class StatementParameters
{
    public CompilerContext context;
    public IMethod method;
    public BasicBlockBuilder block;
    public LNode node;
    public QualifiedName? modulename;
    public Scope scope;
    public BranchLabels branchLabels;

    public StatementParameters(CompilerContext context, IMethod method, BasicBlockBuilder block,
        LNode node, QualifiedName? modulename, Scope scope)
    {
        this.context = context;
        this.method = method;
        this.block = block;
        this.node = node;
        this.modulename = modulename;
        this.scope = scope;
        branchLabels = new();
    }
}