using Backlang.Codeanalysis.Core;
using Backlang.Codeanalysis.Parsing.AST.Statements;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class UnionDeclaration : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Prev;

        var nameToken = iterator.Match(TokenType.Identifier);

        iterator.Match(TokenType.OpenCurly);

        var members = ParsingHelpers.ParseSeperated<VariableStatement>(parser, TokenType.CloseCurly);

        return SyntaxTree.Union(nameToken.Text, members).WithRange(keywordToken, iterator.Current);
    }
}