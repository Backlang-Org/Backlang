using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST;

public sealed class TypeLiteral
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        LNode typeNode;

        if (iterator.IsMatch(TokenType.Identifier))
        {
            var typename = iterator.Match(TokenType.Identifier).Text;
            var args = new LNodeList();

            typeNode = SyntaxTree.Type($"#{typename}", new());

            if (iterator.Current.Type == TokenType.Star)
            {
                iterator.NextToken();

                typeNode = SyntaxTree.Pointer(typeNode);
            }
            else if (iterator.Current.Type == TokenType.OpenSquare)
            {
                iterator.NextToken();

                var dimensions = 1;

                while (iterator.Current.Type == TokenType.Comma)
                {
                    dimensions++;

                    iterator.NextToken();
                }

                iterator.Match(TokenType.CloseSquare);

                typeNode = SyntaxTree.Array(typeNode, dimensions);
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

                typeNode = typeNode.WithArgs(args);
            }
        }
        else if (iterator.IsMatch(TokenType.OpenParen))
        {
            LNode returnType = LNode.Missing;

            iterator.Match(TokenType.OpenParen);

            var parameters = Expression.ParseList(parser, TokenType.CloseParen);

            if (iterator.Current.Type == TokenType.Arrow)
            {
                iterator.NextToken();

                returnType = TypeLiteral.Parse(iterator, parser);
            }

            typeNode = LNode.Call(CodeSymbols.Fn, LNode.List(returnType, LNode.Missing, LNode.Call(CodeSymbols.AltList, parameters)));
        }
        else
        {
            parser.Messages.Add(Message.Error(parser.Document, "Expected Identifier or Function-Signature as TypeLiteral, but got " + iterator.Current.Type, parser.Iterator.Current.Line, parser.Iterator.Current.Column));
            typeNode = LNode.Missing;
            iterator.NextToken();
        }

        return typeNode;
    }
}