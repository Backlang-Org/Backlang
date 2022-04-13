using Backlang.Codeanalysis.Parsing.AST;

namespace Backlang_Compiler.Compiling;

public sealed class PassManager
{
    private readonly List<IVisitor<SyntaxNode>> Passes = new();

    public void AddPass<T>()
        where T : IVisitor<SyntaxNode>, new()
    {
        Passes.Add(new T());
    }

    public CompilationUnit Process(CompilationUnit compilationUnit)
    {
        CompilationUnit result = compilationUnit;
        for (int i = 0; i < Passes.Count; i++)
        {
            var pass = Passes[i];

            result = (CompilationUnit)result.Accept(pass);
        }

        return result;
    }
}