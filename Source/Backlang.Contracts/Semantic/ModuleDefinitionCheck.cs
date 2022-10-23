using Backlang.Codeanalysis.Core;

namespace Backlang.Contracts.Semantic;

internal class ModuleDefinitionCheck : ISemanticCheck
{
    public void Check(CompilationUnit tree, CompilerContext context)
    {
        if (tree.Body.Count(_ => _.Calls(CodeSymbols.Namespace)) > 1)
        {
            var moduleDefinition = tree.Body.First(_ => _.Calls(CodeSymbols.Namespace));

            context.Messages.Add(Message.Warning("A module definition is already defined", moduleDefinition.Range));
        }
    }
}