using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class ParameterDeclaration : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        Annotation.TryParse(parser, out var annotations);

        var name = iterator.Match(TokenType.Identifier);

        iterator.Match(TokenType.Colon);

        var type = TypeLiteral.Parse(iterator, parser);

        LNode defaultValue = LNode.Missing;

        if (iterator.Current.Type == TokenType.EqualsToken)
        {
            iterator.NextToken();

            defaultValue = Expression.Parse(parser);
        }

        return SyntaxTree.Factory.Var(type, name.Text, defaultValue).PlusAttrs(annotations);
    }
}