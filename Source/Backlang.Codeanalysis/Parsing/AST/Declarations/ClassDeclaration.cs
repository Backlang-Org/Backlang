using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class ClassDeclaration : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;

        var attributes = Signature.ParseAttributes(parser);
        var name = iterator.Match(TokenType.Identifier).Text;
        var inheritances = new LNodeList();
        var members = new LNodeList();

        if (iterator.Current.Type == TokenType.Colon)
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
            Annotation.TryParse(parser, out var annotations);

            if (iterator.Current.Type == TokenType.Function)
            {
                members.Add(TypeFunctionDeclaration.Parse(iterator, parser).PlusAttrs(annotations));
            }
            else
            {
                members.Add(TypeFieldDeclaration.Parse(iterator, parser).PlusAttrs(annotations));
            }
        }

        iterator.Match(TokenType.CloseCurly);

        return SyntaxTree.Class(name, inheritances, members).WithAttrs(attributes).WithRange(keywordToken, iterator.Prev);
    }
}