using Furesoft.Core.CodeDom.Compiler.Flow;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Statements;

public class ThrowImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(StatementParameters parameters)
    {
        var valueNode = parameters.node.Args[0].Args[0];
        var constant = parameters.block.AppendInstruction(ConvertConstant(
            GetLiteralType(valueNode, parameters.context, parameters.scope,
            parameters.modulename.Value), valueNode.Value));

        var msg = parameters.block.AppendInstruction(
            Instruction.CreateLoad(GetLiteralType(valueNode, parameters.context,
            parameters.scope, parameters.modulename.Value), constant));

        if (parameters.node.Args[0].Name.Name == "#string")
        {
            var exceptionType = Utils.ResolveType(parameters.context.Binder, typeof(Exception));
            var exceptionCtor = exceptionType.Methods.FirstOrDefault(_ => _.IsConstructor && _.Parameters.Count == 1);

            parameters.block.AppendInstruction(Instruction.CreateNewObject(exceptionCtor, new List<ValueTag> { msg }));
        }

        parameters.block.Flow = UnreachableFlow.Instance;

        return parameters.block;
    }
}