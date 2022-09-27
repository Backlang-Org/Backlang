using Backlang.Codeanalysis.Parsing.AST.Declarations;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST;

public sealed class Signature
{
    public static LNode Parse(Parser parser)
    {
        var iterator = parser.Iterator;

        LNode name;
        if (!TypeLiteral.TryParse(parser, out name))
        {
            //error
            var range = new SourceRange(parser.Document, iterator.Current.Start, iterator.Current.Text.Length);
            parser.Messages.Add(Message.Error(
                $"Expected Identifier, got {iterator.Current.Text}", range));
        }

        LNode returnType = SyntaxTree.Type("none", LNode.List());
        LNodeList generics = new();

        iterator.Match(TokenType.OpenParen);

        var parameters = ParameterDeclaration.ParseList(parser);


        while (iterator.IsMatch(TokenType.Where))
        {
            iterator.NextToken();
            var genericName = LNode.Id(iterator.Match(TokenType.Identifier).Text);
            iterator.Match(TokenType.Colon);
            var bases = new LNodeList();
            do
            {
                if (iterator.IsMatch(TokenType.Comma)) iterator.NextToken();
                bases.Add(TypeLiteral.Parse(iterator, parser));
            } while (iterator.IsMatch(TokenType.Comma));

            generics.Add(LNode.Call(Symbols.Where, LNode.List(genericName, LNode.Call(CodeSymbols.Base, bases))));
        }

        if (iterator.IsMatch(TokenType.Arrow))
        {
            iterator.NextToken();

            returnType = TypeLiteral.Parse(iterator, parser);
        }

        return SyntaxTree.Signature(name, returnType, parameters, generics);
    }
}