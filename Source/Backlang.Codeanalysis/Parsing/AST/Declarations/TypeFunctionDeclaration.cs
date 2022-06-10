using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class TypeFunctionDeclaration
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Match(TokenType.Function);
        var result = Signature.Parse(parser);
        iterator.Match(TokenType.Semicolon);

        return result;
    }
}