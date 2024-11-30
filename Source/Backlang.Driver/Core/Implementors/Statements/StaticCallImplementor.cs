namespace Backlang.Driver.Core.Implementors.Statements;

public class StaticCallImplementor : IStatementImplementor, IExpressionImplementor
{
    public bool CanHandle(LNode node)
    {
        return node.Calls(CodeSymbols.ColonColon);
    }

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, CompilerContext context, Scope scope, QualifiedName? modulename)
    {
        var callee = node.Args[1];

        var type = TypeDeducer.Deduce(node.Args[0], scope, context, modulename.Value);

        return ImplementationStage.AppendCall(context, block, callee, type.Methods, scope, modulename,
            methodName: callee.Name.Name);
    }

    public BasicBlockBuilder Implement(LNode node, BasicBlockBuilder block, CompilerContext context, IMethod method,
        QualifiedName? modulename, Scope scope, BranchLabels branchLabels = null)
    {
        Handle(node, block,
            TypeDeducer.Deduce(node, scope, context, modulename.Value),
            context, scope, modulename);

        return block;
    }
}