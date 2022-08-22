namespace Backlang.Driver.Core.Implementors.Statements;

public class StaticCallImplementor : IStatementImplementor, IExpressionImplementor
{
    public bool CanHandle(LNode node) => node.Calls(CodeSymbols.ColonColon);

    public NamedInstructionBuilder Handle(LNode node, BasicBlockBuilder block,
        IType elementType, CompilerContext context, Scope scope)
    {
        var callee = node.Args[1];
        var typename = ConversionUtils.GetQualifiedName(node.Args[0]);

        var type = (DescribedType)context.Binder.ResolveTypes(typename).FirstOrDefault();

        return ImplementationStage.AppendCall(context, block, callee, type.Methods, scope, callee.Name.Name);
    }

    public BasicBlockBuilder Implement(CompilerContext context, IMethod method, BasicBlockBuilder block,
        LNode node, QualifiedName? modulename, Scope scope)
    {
        Handle(node, block, TypeDeducer.Deduce(node, scope, context), context, scope);

        return block;
    }
}