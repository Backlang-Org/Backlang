using Backlang_Compiler.Parsing;
using Backlang_Compiler.Parsing.AST;
using Backlang_Compiler.Parsing.AST.Expressions;

namespace Backlang_Compiler.Compiling.Passes;

public class ConstantFoldingPass : IPass
{
    public CompilationUnit Process(CompilationUnit obj, PassManager passManager)
    {
        if (obj is Expression expr)
            return new LiteralNode(Evaluate(expr));

        return obj;
    }

    private int Evaluate(Expression tree)
    {
        if (tree is BinaryExpression expr)
            switch (expr.OperatorToken.Text)
            {
                case "+":
                    return Evaluate(expr.Left) + Evaluate(expr.Right);

                case "-":
                    return Evaluate(expr.Left) - Evaluate(expr.Right);

                case "*":
                    return Evaluate(expr.Left) * Evaluate(expr.Right);

                case "/":
                    return Evaluate(expr.Left) / Evaluate(expr.Right);
            }
        else if (tree is LiteralNode lit)
        {
            return (int)lit.Value;
        }

        return 0;
    }
}
