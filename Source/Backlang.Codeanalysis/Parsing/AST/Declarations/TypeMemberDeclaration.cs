using Backlang.Codeanalysis.Parsing.AST.Statements;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class TypeMemberDeclaration
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        Annotation.TryParse(parser, out var annotations);
        Modifier.TryParse(parser, out var modifiers);

        LNode declaration = LNode.Missing;

        if (iterator.IsMatch(TokenType.Function))
        {
            declaration = ParseFunction(iterator, parser);
        }
        else if (iterator.IsMatch(TokenType.Property))
        {
            declaration = ParseProperty(iterator, parser);
        }
        else if (iterator.IsMatch(TokenType.Let))
        {
            declaration = ParseField(iterator, parser);
        }
        else
        {
            parser.Messages.Add(Message.Error(parser.Document, $"Expected Function, Property or Field-declaration for Type, but got {iterator.Current}", iterator.Current.Line, iterator.Current.Column));
            iterator.NextToken();
        }

        return declaration.PlusAttrs(annotations).PlusAttrs(modifiers);
    }

    public static LNode ParseField(TokenIterator iterator, Parser parser)
    {
        iterator.Match(TokenType.Let);
        return VariableStatement.Parse(iterator, parser);
    }

    public static LNode ParseFunction(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Match(TokenType.Function);
        var result = Signature.Parse(parser);
        iterator.Match(TokenType.Semicolon);

        return result.WithRange(keywordToken, iterator.Prev);
    }

    public static LNode ParseProperty(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Match(TokenType.Property);
        LNode type = LNode.Missing;
        LNode value = LNode.Missing;
        var nameToken = iterator.Match(TokenType.Identifier);
        var name = LNode.Id(nameToken.Text);

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

        return SyntaxTree.Property(type, name, value).WithRange(keywordToken, iterator.Prev);
    }
}