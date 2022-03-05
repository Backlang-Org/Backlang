using Backlang_Compiler.Parsing.AST;

namespace Backlang_Compiler.Compiling;

public interface IPass
{
    SyntaxNode Process(SyntaxNode obj, PassManager passManager);
}