using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public sealed class TryStatement : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        // try {} catch (Exception e) {} catch (Exception e) {} finally {}
        var keywordToken = iterator.Prev;
        var body = Statement.ParseOneOrBlock(parser);
        LNodeList catches = new(LNode.Missing);

        if (iterator.Current.Type != TokenType.Catch)
        {
            parser.Messages.Add(Message.Error(parser.Document, "Expected at least one catch block at try statement", parser.Iterator.Current.Line, parser.Iterator.Current.Column));
        }

        while (iterator.Current.Type == TokenType.Catch)
        {
            iterator.NextToken();

            catches.Add(ParseCatch(parser));
        }

        LNodeList finallly = new(LNode.Missing);
        if (iterator.IsMatch(TokenType.Finally))
        {
            iterator.Match(TokenType.Finally);
            finallly = Statement.ParseOneOrBlock(parser);
        }

        return SyntaxTree.Try(body, catches, finallly).WithRange(keywordToken, iterator.Prev);
    }

    private static LNode ParseCatch(Parser parser)
    {
        parser.Iterator.Match(TokenType.OpenParen);
        var exceptionValueName = LNode.Id(parser.Iterator.Match(TokenType.Identifier).Text);
        parser.Iterator.Match(TokenType.Colon);
        var exceptionType = LNode.Id(parser.Iterator.Match(TokenType.Identifier).Text);
        parser.Iterator.Match(TokenType.CloseParen);

        var body = Statement.ParseOneOrBlock(parser);

        return SyntaxTree.Catch(exceptionType, exceptionValueName, body);
    }
}