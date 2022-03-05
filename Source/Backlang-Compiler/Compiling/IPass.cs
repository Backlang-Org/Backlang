using Backlang_Compiler.Parsing.AST;
using Backlang_Compiler.Compiling;

namespace Backlang_Compiler.Compiling;

public interface IPass
{
    CompilationUnit Process(CompilationUnit obj, PassManager passManager);
}
