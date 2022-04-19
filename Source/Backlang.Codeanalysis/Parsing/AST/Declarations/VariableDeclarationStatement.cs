using Backlang.Codeanalysis.Parsing.AST.Statements;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class VariableDeclarationStatement : Statement, IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        bool isMutable = false;
        TypeLiteral? type = null;
        LNode value = LNode.Missing;

        if (iterator.Current.Type == TokenType.Mutable)
        {
            isMutable = true;
            iterator.NextToken();
        }

        var nameToken = iterator.Match(TokenType.Identifier);

        if (iterator.Current.Type == TokenType.Colon)
        {
            iterator.NextToken();

            type = TypeLiteral.Parse(iterator, parser);
        }

        if (iterator.Current.Type == TokenType.EqualsToken)
        {
            iterator.NextToken();

            value = Expression.Parse(parser);
        }

        iterator.Match(TokenType.Semicolon);

        return new VariableDeclarationStatement(nameToken.Text, type, isMutable, value);
    }
}