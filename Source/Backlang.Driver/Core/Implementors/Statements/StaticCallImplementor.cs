namespace Backlang.Driver.Core.Implementors.Statements;

public class StaticCallImplementor : IStatementImplementor, IExpressionImplementor
{
    public bool CanHandle(LNode node) => node.Calls(CodeSymbols.ColonColon);

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, CompilerContext context, Scope scope, QualifiedName? modulename)
    {
        var callee = node.Args[1];

        var type = TypeDeducer.Deduce(node.Args[0], scope, context, modulename.Value);

        return ImplementationStage.AppendCall(context, block, callee, type.Methods, scope, modulename, methodName: callee.Name.Name);
    }

    public BasicBlockBuilder Implement(CompilerContext context, IMethod method, BasicBlockBuilder block,
        LNode node, QualifiedName? modulename, Scope scope)
    {
        Handle(node, block, TypeDeducer.Deduce(node, scope, context, modulename.Value), context, scope, modulename);

        return block;
    }
}