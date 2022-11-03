namespace Backlang.Driver.Core.Implementors.Statements;

public class PrintOrPrintlnImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(CompilerContext context, IMethod method, BasicBlockBuilder block, LNode node, QualifiedName? modulename, Scope scope)
    {
        var deducedArg = TypeDeducer.Deduce(node.Args[0], scope, context, modulename.Value);
        var functionName = node.Calls("println") ? "WriteLine" : "Write";

        var m = context.Binder.FindFunction($"System.Console::{functionName}({deducedArg})");

        ImplementationStage.AppendCall(context, block, node, m, scope, modulename.Value);

        return block;
    }
}