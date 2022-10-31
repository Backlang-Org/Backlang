using Backlang.Codeanalysis.Core;
using Backlang.Contracts.Semantic;

namespace Backlang.Contracts;

public static class SemanticChecker
{
    private static readonly List<ISemanticCheck> _semanticChecks = new() {
        new ModuleDefinitionCheck(),
        new ImportCheck(),
        new TypenameCheck(),
        new ModifierCheck(),
        new InterfaceNameCheck()
    };

    public static void Do(CompilationUnit tree, CompilerContext context)
    {
        foreach (var check in _semanticChecks)
        {
            check.Check(tree, context);
        }
    }
}