using Backlang.Codeanalysis.Core;

namespace Backlang.Contracts.Semantic;

internal class ModuleDefinitionCheck : ISemanticCheck
{
    public void Check(CompilationUnit tree, List<Message> messages)
    {
        if (tree.Body.Count(_ => _.Calls(CodeSymbols.Namespace)) > 1)
        {
            var moduleDefinition = tree.Body.First(_ => _.Calls(CodeSymbols.Namespace));

            messages.Add(Message.Warning("A module definition is already defined", moduleDefinition.Range));
        }
    }
}