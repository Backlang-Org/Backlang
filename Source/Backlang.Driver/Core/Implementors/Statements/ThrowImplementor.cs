using Furesoft.Core.CodeDom.Compiler.Flow;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Statements;

public class ThrowImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(LNode node, BasicBlockBuilder block, CompilerContext context, IMethod method,
        QualifiedName? modulename, Scope scope, BranchLabels branchLabels = null)
    {
        var valueNode = node.Args[0].Args[0];
        var constant = block.AppendInstruction(ConvertConstant(
            GetLiteralType(valueNode, context, scope,
                modulename.Value), valueNode.Value));

        var msg = block.AppendInstruction(
            Instruction.CreateLoad(GetLiteralType(valueNode, context,
                scope, modulename.Value), constant));

        if (node.Args[0].Name.Name == "#string")
        {
            var exceptionType = Utils.ResolveType(context.Binder, typeof(Exception));
            var exceptionCtor = exceptionType.Methods.FirstOrDefault(_ => _.IsConstructor && _.Parameters.Count == 1);

            block.AppendInstruction(Instruction.CreateNewObject(exceptionCtor, new List<ValueTag> { msg }));
        }

        block.Flow = UnreachableFlow.Instance;

        return block;
    }
}