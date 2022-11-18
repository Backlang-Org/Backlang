using Backlang.Codeanalysis.Parsing.AST.Statements;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class TypeMemberDeclaration : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        _ = Annotation.TryParse(parser, out var annotations);
        _ = Modifier.TryParse(parser, out var modifiers);

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
            var range = new SourceRange(parser.Document, iterator.Current.Start, iterator.Current.Text.Length);

            parser.AddError(new(Core.ErrorID.UnexpecedTypeMember, iterator.Current.Text), range);
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

        if (iterator.IsMatch(TokenType.Colon))
        {
            iterator.NextToken();

            type = TypeLiteral.Parse(iterator, parser);
        }

        iterator.Match(TokenType.OpenCurly);

        LNode getter = LNode.Missing;
        LNode setter = LNode.Missing;

        var needModifier = false;

        _ = Modifier.TryParse(parser, out var modifier);
        if (iterator.IsMatch(TokenType.Get))
        {
            iterator.NextToken();
            var args = LNode.List();
            if (iterator.IsMatch(TokenType.Semicolon))
            {
                iterator.NextToken();
            }
            else
            {
                args.Add(Statement.ParseBlock(parser));
            }
            getter = LNode.Call(CodeSymbols.get, args).WithAttrs(modifier);
            needModifier = true;
        }

        if (needModifier)
        {
            _ = Modifier.TryParse(parser, out modifier);
        }

        if (iterator.IsMatch(TokenType.Set))
        {
            iterator.NextToken();
            var args = LNode.List();
            if (iterator.IsMatch(TokenType.Semicolon))
            {
                iterator.NextToken();
            }
            else
            {
                args.Add(Statement.ParseBlock(parser));
            }
            setter = LNode.Call(CodeSymbols.set, args).WithAttrs(modifier);
        }
        else if (iterator.IsMatch(TokenType.Init))
        {
            iterator.NextToken();
            var args = LNode.List();
            if (iterator.IsMatch(TokenType.Semicolon))
            {
                iterator.NextToken();
            }
            else
            {
                args.Add(Statement.ParseBlock(parser));
            }
            setter = LNode.Call(Symbols.Init, args).WithAttrs(modifier);
        }

        iterator.Match(TokenType.CloseCurly);

        if (iterator.IsMatch(TokenType.EqualsToken))
        {
            iterator.NextToken();

            value = Expression.Parse(parser);

            iterator.Match(TokenType.Semicolon);
        }

        return SyntaxTree.Property(type, name, getter, setter, value).WithRange(keywordToken, iterator.Prev);
    }
}