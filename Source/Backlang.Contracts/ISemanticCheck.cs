namespace Backlang.Codeanalysis.Core;

public interface ISemanticCheck
{
    void Check(CompilationUnit tree, Contracts.CompilerContext context);
}