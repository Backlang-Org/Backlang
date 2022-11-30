namespace Backlang.Driver.Core.Implementors.Statements;

public class PrintOrPrintlnImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(StatementParameters parameters)
    {
        var deducedArg = TypeDeducer.Deduce(parameters.node.Args[0], parameters.scope,
            parameters.context, parameters.modulename.Value);
        var functionName = parameters.node.Calls("println") ? "WriteLine" : "Write";

        var printFunction = parameters.context.Binder.FindFunction($"System.Console::{functionName}({deducedArg})");

        ImplementationStage.AppendCall(parameters.context, parameters.block, parameters.node,
            printFunction, parameters.scope, parameters.modulename.Value);

        return parameters.block;
    }
}