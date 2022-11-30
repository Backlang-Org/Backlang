using Backlang.Contracts.Scoping.Items;
using Backlang.Driver.Core.Instructions;
using static Backlang.Driver.Compiling.Stages.CompilationStages.ImplementationStage;

namespace Backlang.Driver.Core.Implementors.Statements;

public class CallImplementor : IStatementImplementor
{
    public static void AppendDiscardReturnValue(BasicBlockBuilder block, IType type)
    {
        if (type.FullName.ToString() != "System.Void")
        {
            //Discard value if its not been stored anywhere
            block.AppendInstruction(new PopInstructionPrototype().Instantiate(new List<ValueTag>()));
        }
    }

    public BasicBlockBuilder Implement(StatementParameters parameters)
    {
        if (parameters.node is ("'.", var target, var callee) && target is ("this", _))
        {
            if (parameters.method.IsStatic)
            {
                parameters.context.AddError(parameters.node, "'this' cannot be used in static methods");
                return parameters.block;
            }

            if (parameters.scope.TryGet<FunctionScopeItem>(callee.Name.Name, out var fsi))
            {
                AppendCall(parameters.context, parameters.block, callee, fsi.Overloads,
                    parameters.scope, parameters.modulename);

                AppendDiscardReturnValue(parameters.block, fsi.Overloads[0].ReturnParameter.Type);
            }
            else
            {
                parameters.context.AddError(parameters.node, new(Codeanalysis.Core.ErrorID.CannotFindFunction, callee.Name.Name));
            }
        }
        else
        {
            // ToDo: other things and so on...
        }

        return parameters.block;
    }
}