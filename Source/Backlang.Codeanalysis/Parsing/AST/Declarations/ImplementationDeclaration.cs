using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class ImplementationDeclaration : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;
        var targets = new LNodeList();

        while (iterator.Current.Type != TokenType.OpenCurly && !parser.Iterator.IsMatch(TokenType.EOF))
        {
            if (iterator.Peek(1).Type == TokenType.RangeOperator)
            {
                targets.Add(Expression.Parse(parser));
            }
            else
            {
                targets.Add(TypeLiteral.Parse(iterator, parser));
            }

            if (iterator.Current.Type != TokenType.OpenCurly)
            {
                iterator.Match(TokenType.Comma);
            }
        }


        LNode target;
        if (targets.Count == 1)
        {
            target = targets[0];
        }
        else
        {
            //ToDo: need to be fixed
            target = LNode.Call(Symbols.ToExpand, targets);
        }

        iterator.Match(TokenType.OpenCurly);

        LNodeList body = new();
        while (!parser.Iterator.IsMatch(TokenType.EOF) && iterator.Current.Type != TokenType.CloseCurly)
        {
            _ = Annotation.TryParse(parser, out var annotations);
            _ = Modifier.TryParse(parser, out var modifiers);

            body.Add(parser.InvokeParsePoint(parser.DeclarationParsePoints).PlusAttrs(annotations).PlusAttrs(modifiers));
        }

        iterator.Match(TokenType.CloseCurly);

        return SyntaxTree.ImplDecl(target, body).WithRange(keywordToken, iterator.Prev);
    }
}