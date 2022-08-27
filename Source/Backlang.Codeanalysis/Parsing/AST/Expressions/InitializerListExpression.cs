using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Expressions;

public sealed class InitializerListExpression : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var elements = new LNodeList();

        do
        {
            if (iterator.Current.Type == (TokenType.CloseSquare))
            {
                break;
            }

            elements.Add(Expression.Parse(parser));

            if (iterator.Current.Type != TokenType.CloseSquare)
            {
                iterator.Match(TokenType.Comma);
            }
        } while (iterator.Current.Type != (TokenType.CloseSquare));

        iterator.Match(TokenType.CloseSquare);

        return SyntaxTree.ArrayInstantiation(elements);
    }
}