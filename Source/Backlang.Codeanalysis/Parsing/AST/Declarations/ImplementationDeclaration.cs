using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class ImplementationDeclaration : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        if (iterator.Current.Type == TokenType.For || iterator.Current.Type == TokenType.Of)
        {
            iterator.NextToken();

            bool isStatic = false;
            if (iterator.Current.Type == TokenType.Static)
            {
                isStatic = true;
                iterator.NextToken();
            }

            LNode target = null;

            if (iterator.Peek(1).Type == TokenType.RangeOperator)
            {
                target = Expression.Parse(parser);
            }
            else
            {
                target = TypeLiteral.Parse(iterator, parser);
            }

            iterator.Match(TokenType.OpenCurly);

            LNodeList body = new();
            while (iterator.Current.Type != TokenType.EOF && iterator.Current.Type != TokenType.CloseCurly)
            {
                body.Add(parser.InvokeParsePoint(parser.DeclarationParsePoints));
            }

            iterator.Match(TokenType.CloseCurly);

            return SyntaxTree.ImplDecl(target, body, isStatic);
        }

        parser.Messages.Add(
            Message.Error(parser.Document,
                "Expected For Or Of", iterator.Current.Line, iterator.Current.Column));

        return LNode.Missing;
    }
}