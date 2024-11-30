using Backlang.Contracts;

namespace Backlang.Codeanalysis.Core;

public interface ISemanticCheck
{
    void Check(CompilationUnit tree, CompilerContext context);
}