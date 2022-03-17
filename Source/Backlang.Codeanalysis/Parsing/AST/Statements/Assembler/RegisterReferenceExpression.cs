namespace Backlang.Codeanalysis.Parsing.AST.Statements.Assembler;

public class RegisterReferenceExpression : Expression, IParsePoint<Expression>
{
    public string RegisterName { get; set; }

    public static Expression Parse(TokenIterator iterator, Parser parser)
    {
        var expr = new RegisterReferenceExpression();

        expr.RegisterName = iterator.Peek(-1).Text;

        return expr;
    }
}