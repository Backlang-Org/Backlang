using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class StructDeclaration : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;
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
            Modifier.TryParse(parser, out var modifiers);

            if (iterator.Current.Type == TokenType.Function)
            {
                members.Add(TypeFunctionDeclaration.Parse(iterator, parser).PlusAttrs(annotations).PlusAttrs(modifiers));
            }
            else if (iterator.Current.Type == TokenType.Property)
            {
                members.Add(TypePropertyDeclaration.Parse(iterator, parser).PlusAttrs(annotations).PlusAttrs(modifiers));
            }
            else
            {
                members.Add(TypeFieldDeclaration.Parse(iterator, parser).PlusAttrs(annotations).PlusAttrs(modifiers));
            }
        }

        iterator.Match(TokenType.CloseCurly);

        return SyntaxTree.Struct(name, inheritances, members)
            .WithRange(keywordToken, iterator.Prev);
    }
}