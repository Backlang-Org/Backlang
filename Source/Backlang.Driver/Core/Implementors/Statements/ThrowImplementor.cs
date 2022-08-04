using Backlang.Contracts;
using Backlang.Contracts.Scoping;
using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Flow;
using Loyc.Syntax;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Statements;

public class ThrowImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(CompilerContext context, IMethod method, BasicBlockBuilder block,
        LNode node, QualifiedName? modulename, Scope scope)
    {
        var valueNode = node.Args[0].Args[0];
        var constant = block.AppendInstruction(ConvertConstant(
            GetLiteralType(valueNode, context, scope), valueNode.Value));

        var msg = block.AppendInstruction(
            Instruction.CreateLoad(GetLiteralType(valueNode, context, scope), constant));

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