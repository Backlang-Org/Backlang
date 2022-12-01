using Backlang.Codeanalysis.Core;
using Backlang.Driver;

namespace Backlang.Contracts.Semantic;

internal class VariableTypeCheck : ISemanticCheck
{
    public void Check(CompilationUnit tree, CompilerContext context)
    {
        var letNodes = tree.Body.SelectMany(_ => _.Descendants()).Where(_ => _.Calls(CodeSymbols.Var)).ToArray();

        foreach (var node in letNodes)
        {
            if (node is (_, (_, (_, var type)), (_, _, var value)))
            {
                if (type.Name.Name == "" && value.Calls(CodeSymbols.Void))
                {
                    context.AddError(node, ErrorID.CannotDeduceType);
                }
            }
        }
    }
}