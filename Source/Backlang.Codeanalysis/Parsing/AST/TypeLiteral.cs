using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST;

public sealed class TypeLiteral
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        LNode typeNode;
        var typeToken = iterator.Current;

        if (iterator.IsMatch(TokenType.Identifier))
        {
            var typename = iterator.Match(TokenType.Identifier).Text;
            var args = new LNodeList();

            typeNode = SyntaxTree.Type(typename, new()).WithRange(typeToken);

            if (iterator.IsMatch(TokenType.Star))
            {
                iterator.NextToken();

                typeNode = SyntaxTree.Pointer(typeNode).WithRange(typeToken, iterator.Prev);
            }
            if (iterator.IsMatch(TokenType.Ampersand))
            {
                iterator.NextToken();

                typeNode = SyntaxTree.RefType(typeNode).WithRange(typeToken, iterator.Prev);
            }
            else if (iterator.IsMatch(TokenType.OpenSquare))
            {
                iterator.NextToken();

                var dimensions = 1;

                while (iterator.IsMatch(TokenType.Comma))
                {
                    dimensions++;

                    iterator.NextToken();
                }

                iterator.Match(TokenType.CloseSquare);

                typeNode = SyntaxTree.Array(typeNode, dimensions).WithRange(typeToken, iterator.Prev);
            }
            else if (iterator.IsMatch(TokenType.LessThan))
            {
                iterator.NextToken();

                while (!iterator.IsMatch(TokenType.GreaterThan))
                {
                    if (iterator.IsMatch(TokenType.Identifier))
                    {
                        args.Add(Parse(iterator, parser));
                    }

                    if (!iterator.IsMatch(TokenType.GreaterThan))
                    {
                        iterator.Match(TokenType.Comma);
                    }
                }

                iterator.Match(TokenType.GreaterThan);

                typeNode = SyntaxTree.Type(typename, args).WithRange(typeToken, parser.Iterator.Prev);
            }
        }
        else if (iterator.IsMatch(TokenType.None))
        {
            typeNode = SyntaxTree.Type("none", LNode.List()).WithRange(typeToken); // Missing is the normal type for none
            iterator.NextToken();
        }
        else if (iterator.IsMatch(TokenType.OpenParen))
        {
            iterator.Match(TokenType.OpenParen);

            var parameters = Expression.ParseList(parser, TokenType.CloseParen);

            if (iterator.Current.Type == TokenType.Arrow)
            {
                iterator.NextToken();

                var returnType = TypeLiteral.Parse(iterator, parser);

                typeNode = SyntaxTree.Factory.Call(CodeSymbols.Fn,
                    LNode.List(returnType, LNode.Missing, LNode.Call(CodeSymbols.AltList, parameters))).WithRange(typeToken, iterator.Prev);
            }
            else
            {
                typeNode = SyntaxTree.Factory.Tuple(parameters).WithRange(typeToken, iterator.Prev);
            }
        }
        else
        {
            parser.AddError("Expected Identifier, TupleType or Function-Signature as TypeLiteral, but got " + iterator.Current.Type);

            typeNode = LNode.Missing;
            iterator.NextToken();
        }

        return typeNode;
    }

    public static bool TryParse(Parser parser, out LNode node)
    {
        var cursor = parser.Iterator.Position;
        node = Parse(parser.Iterator, parser);

        if (node == LNode.Missing)
        {
            parser.Iterator.Position = cursor;
            return false;
        }

        return true;
    }
}