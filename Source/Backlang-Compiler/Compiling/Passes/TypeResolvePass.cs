namespace Backlang_Compiler.Compiling.Passes;

public class TypeResolvePass : IPass
{
    public CodeObject Process(CodeObject obj, PassManager passManager)
    {
        if (obj is VarDecl varDecl && varDecl.Type == null)
            ResolveVarType(varDecl);

        return obj;
    }
}
