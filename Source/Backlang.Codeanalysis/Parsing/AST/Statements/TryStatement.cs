using Backlang.Codeanalysis.Core;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public sealed class TryStatement : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;
        var body = Statement.ParseOneOrBlock(parser);
        LNodeList catches = new(LNode.Missing);

        if (iterator.Current.Type != TokenType.Catch)
        {
            var range = new SourceRange(parser.Document, iterator.Current.Start, iterator.Current.Text.Length);

            parser.AddError(new LocalizableString(ErrorID.NoCatchBlock), range);
        }

        while (iterator.Current.Type == TokenType.Catch)
        {
            iterator.NextToken();

            catches.Add(ParseCatch(parser));
        }

        LNode finallly = LNode.Missing;
        if (iterator.IsMatch(TokenType.Finally))
        {
            iterator.Match(TokenType.Finally);
            finallly = Statement.ParseOneOrBlock(parser);
        }

        return SyntaxTree.Try(body, SyntaxTree.Factory.AltList(catches), finallly)
            .WithRange(keywordToken, iterator.Prev);
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