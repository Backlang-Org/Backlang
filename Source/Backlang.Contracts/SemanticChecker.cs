using Backlang.Codeanalysis.Core;
using Backlang.Contracts.Semantic;

namespace Backlang.Contracts;

public static class SemanticChecker
{
    private static readonly List<ISemanticCheck> _semanticChecks = new() {
        new ModuleDefinitionCheck(),
        new ImportCheck()
    };

    public static void Do(CompilationUnit tree)
    {
        foreach (var check in _semanticChecks)
        {
            check.Check(tree);
        }
    }
}