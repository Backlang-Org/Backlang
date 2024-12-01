using Backlang.Codeanalysis.Parsing.AST;

namespace BacklangC.Semantic;

public interface ISemanticCheck
{
    void Check(CompilationUnit tree, Driver context);
}