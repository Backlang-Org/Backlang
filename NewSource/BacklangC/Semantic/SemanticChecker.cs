using Backlang.Codeanalysis.Parsing.AST;
using BacklangC.Semantic.Checks;

namespace BacklangC.Semantic;

public static class SemanticChecker
{
    private static readonly List<ISemanticCheck> SemanticChecks =
    [
        new ModuleDefinitionCheck(),
        new ImportCheck(),
        new TypenameCheck(),
        new ModifierCheck(),
        new InterfaceNameCheck()
    ];

    public static void Do(CompilationUnit tree, Driver context)
    {
        foreach (var check in SemanticChecks)
        {
            check.Check(tree, context);
        }
    }
}