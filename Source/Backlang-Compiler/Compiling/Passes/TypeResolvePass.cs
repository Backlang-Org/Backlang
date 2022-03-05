using Backlang_Compiler.Parsing.AST;

namespace Backlang_Compiler.Compiling.Passes;

public class TypeResolvePass : IPass
{
    public SyntaxNode Process(SyntaxNode obj, PassManager passManager)
    {
        /*if (obj is VarDecl varDecl && varDecl.Type == null)
            ResolveVarType(varDecl);
        */

        return obj;
    }
}