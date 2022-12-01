namespace Backlang.Driver.Core.Implementors.Statements;

public class PrintOrPrintlnImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(LNode node, BasicBlockBuilder block, CompilerContext context, IMethod method, QualifiedName? modulename, Scope scope, BranchLabels branchLabels = null)
    {
        var deducedArg = TypeDeducer.Deduce(node.Args[0], scope,
            context, modulename.Value);
        var functionName = node.Calls("println") ? "WriteLine" : "Write";

        var printFunction = context.Binder.FindFunction($"System.Console::{functionName}({deducedArg})");

        ImplementationStage.AppendCall(context, block, node,
            printFunction, scope, modulename.Value);

        return block;
    }
}