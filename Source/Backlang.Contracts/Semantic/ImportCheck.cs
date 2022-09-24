using Backlang.Codeanalysis.Core;

namespace Backlang.Contracts.Semantic;

internal class ImportCheck : ISemanticCheck
{
    public void Check(CompilationUnit tree, List<Message> messages)
    {
        for (var i = 0; i < tree.Body.Count; i++)
        {
            var node = tree.Body[i];

            if (i > 0 && !tree.Body[i - 1].Calls(CodeSymbols.Namespace) && node.Calls(CodeSymbols.Import))
            {
                messages.Add(Message.Warning("Imports should be before module definition", node.Range));
            }
        }
    }
}