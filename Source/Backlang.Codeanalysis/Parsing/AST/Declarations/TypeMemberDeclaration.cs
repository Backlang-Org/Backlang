using Backlang.Codeanalysis.Parsing.AST.Statements;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class TypeMemberDeclaration
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        var keywordToken = iterator.Match(TokenType.Let);

        return VariableStatement.Parse(iterator, parser)
            .WithRange(keywordToken, iterator.Peek(-1));
    }
}