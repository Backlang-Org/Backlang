namespace Backlang.Codeanalysis.Parsing.AST.Expressions;

public class NameOfExpression : Expression, IParsePoint<Expression>
{
    public static Expression Parse(TokenIterator iterator, Parser parser)
    {
        iterator.Match(TokenType.OpenParen);

        var name = NameExpression.Parse(iterator, parser);

        iterator.Match(TokenType.CloseParen);

        if (name is NameExpression ne)
        {
            return new LiteralNode(ne.Name);
        }
        else if (name is ArrayAccessExpression aae)
        {
            return new LiteralNode(aae.Name.Name);
        }
        else if (name is CallExpression ce)
        {
            return new LiteralNode(ce.Name);
        }

        return new LiteralNode("");
    }
}