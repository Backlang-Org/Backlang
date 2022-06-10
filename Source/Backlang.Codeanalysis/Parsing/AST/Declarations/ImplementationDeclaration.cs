using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class ImplementationDeclaration : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;

        bool isStatic = false;
        if (iterator.Current.Type == TokenType.Static)
        {
            isStatic = true;
            iterator.NextToken();
        }

        LNode target = null;
        var targets = new LNodeList();

        while (iterator.Current.Type != TokenType.OpenCurly)
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
        while (iterator.Current.Type != TokenType.EOF && iterator.Current.Type != TokenType.CloseCurly)
        {
            body.Add(parser.InvokeParsePoint(parser.DeclarationParsePoints));
        }

        iterator.Match(TokenType.CloseCurly);

        return SyntaxTree.ImplDecl(target, body, isStatic).WithRange(keywordToken, iterator.Prev);
    }
}