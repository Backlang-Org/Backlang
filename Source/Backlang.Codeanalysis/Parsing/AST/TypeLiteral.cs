using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST;

public sealed class TypeLiteral
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var typenameToken = iterator.Match(TokenType.Identifier);
        var typename = typenameToken.Text;
        var args = new LNodeList();

        var typeNode = SyntaxTree.Type($"#{typename}", new());

        if (iterator.Current.Type == TokenType.Star)
        {
            iterator.NextToken();

            return SyntaxTree.Pointer(typeNode).WithRange(iterator.Peek(-1));
        }
        else if (iterator.Current.Type == TokenType.OpenSquare)
        {
            var openSquareToken = iterator.NextToken();

            var dimensions = 1;

            while (iterator.Current.Type == TokenType.Comma)
            {
                dimensions++;

                iterator.NextToken();
            }

            iterator.Match(TokenType.CloseSquare);

            return SyntaxTree.Array(typeNode, dimensions).WithRange(openSquareToken, iterator.Peek(-1));
        }
        else if (iterator.Current.Type == TokenType.LessThan)
        {
            iterator.NextToken();

            while (iterator.Current.Type != TokenType.GreaterThan)
            {
                if (iterator.Current.Type == TokenType.Identifier)
                {
                    args.Add(Parse(iterator, parser));
                }

                if (iterator.Current.Type != TokenType.GreaterThan)
                {
                    iterator.Match(TokenType.Comma);
                }
            }

            iterator.Match(TokenType.GreaterThan);

            return typeNode.WithArgs(args).WithRange(typenameToken, iterator.Peek(-1));
        }

        return typeNode;
    }
}