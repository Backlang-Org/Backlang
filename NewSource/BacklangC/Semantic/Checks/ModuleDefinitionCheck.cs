using Backlang.Codeanalysis.Parsing;
using Backlang.Codeanalysis.Parsing.AST;
using Loyc.Syntax;

namespace BacklangC.Semantic.Checks;

internal class ModuleDefinitionCheck : ISemanticCheck
{
    public void Check(CompilationUnit tree, Driver context)
    {
        if (tree.Body.Count(_ => _.Calls(CodeSymbols.Namespace)) > 1)
        {
            var moduleDefinition = tree.Body.First(_ => _.Calls(CodeSymbols.Namespace));

            context.Messages.Add(Message.Warning("A module definition is already defined", moduleDefinition.Range));
        }
    }
}