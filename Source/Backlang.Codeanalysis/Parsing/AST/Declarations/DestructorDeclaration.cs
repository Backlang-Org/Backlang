using Backlang.Codeanalysis.Parsing.AST.Statements;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public class DestructorDeclaration : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;

        iterator.Match(TokenType.OpenParen);

        var parameters = Signature.ParseParameterDeclarations(iterator, parser);

        iterator.Match(TokenType.CloseParen);

        var code = Statement.ParseBlock(parser);

        return SyntaxTree.Destructor(parameters, code).WithRange(keywordToken, iterator.Prev);
    }
}