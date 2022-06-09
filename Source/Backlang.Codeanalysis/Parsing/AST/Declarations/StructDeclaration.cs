using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class StructDeclaration : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var attributes = Signature.ParseAttributes(parser);
        var name = iterator.Match(TokenType.Identifier).Text;
        var inheritances = new LNodeList();
        var members = new LNodeList();

        if(iterator.Current.Type == TokenType.Colon)
        {
            do
            {
                iterator.NextToken();
                inheritances.Add(Expression.Parse(parser));
            } while (iterator.Current.Type == TokenType.Comma);
        }

        iterator.Match(TokenType.OpenCurly);

        while (iterator.Current.Type != TokenType.CloseCurly)
        {
            if (iterator.Current.Type == TokenType.Function)
            {
                members.Add(TypeFunctionDeclaration.Parse(iterator, parser));
            }
            else
            {
                members.Add(TypeFieldDeclaration.Parse(iterator, parser));
            }
        }

        iterator.Match(TokenType.CloseCurly);

        return SyntaxTree.Struct(name, inheritances, members).WithAttrs(attributes);
    }
}