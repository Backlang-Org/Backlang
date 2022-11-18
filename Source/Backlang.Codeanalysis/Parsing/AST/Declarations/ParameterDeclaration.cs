using Backlang.Codeanalysis.Core;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class ParameterDeclaration : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        Annotation.TryParse(parser, out var annotations);

        var keywordToken = iterator.Current;
        var name = iterator.Match(TokenType.Identifier);

        bool assertNotNull = false;
        if(iterator.ConsumeIfMatch(TokenType.Exclamation)) {
            assertNotNull = true;
        }

        iterator.Match(TokenType.Colon);

        var type = TypeLiteral.Parse(iterator, parser);

        LNode defaultValue = LNode.Missing;

        if (iterator.Current.Type == TokenType.EqualsToken)
        {
            iterator.NextToken();

            defaultValue = Expression.Parse(parser);
        }

        if(assertNotNull)  {
           annotations = annotations.Add(LNode.Id(Symbols.AssertNonNull));
        }

        return SyntaxTree.Factory.Var(type, name.Text, defaultValue).PlusAttrs(annotations)
            .WithRange(keywordToken, iterator.Prev);
    }

    public static LNodeList ParseList(Parser parser)
    {
        return ParsingHelpers.ParseSeperated<ParameterDeclaration>(parser, TokenType.CloseParen);
    }
}