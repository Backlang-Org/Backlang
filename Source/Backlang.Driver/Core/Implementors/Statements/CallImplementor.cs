using Backlang.Contracts.Scoping.Items;
using Backlang.Driver.Core.Instructions;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Statements;

public class CallImplementor : IStatementImplementor
{
    public BasicBlockBuilder Implement(CompilerContext context, IMethod method, BasicBlockBuilder block,
        LNode node, QualifiedName? modulename, Scope scope)
    {
        if (node is ("'.", var target, var callee) && target is ("this", _))
        {
            if (method.IsStatic)
            {
                context.AddError(node, "'this' cannot be used in static methods");
                return block;
            }

            if (scope.TryGet<FunctionScopeItem>(callee.Name.Name, out var fsi))
            {
                AppendCall(context, block, callee, fsi.Overloads, scope, modulename);

                AppendDiscardReturnValue(block, fsi.Overloads[0].ReturnParameter.Type);
            }
            else
            {
                

                context.AddError(node, new(Codeanalysis.Core.ErrorID.CannotFindFunction, callee.Name.Name));
            }
        }
        else
        {
            // ToDo: other things and so on...
        }

        return block;
    }

    public static void AppendDiscardReturnValue(BasicBlockBuilder block, IType type)
    {
        if (type.FullName.ToString() != "System.Void")
        {
            //Discard value if its not been stored anywhere
            block.AppendInstruction(new PopInstructionPrototype().Instantiate(new List<ValueTag>()));
        }
    }
}