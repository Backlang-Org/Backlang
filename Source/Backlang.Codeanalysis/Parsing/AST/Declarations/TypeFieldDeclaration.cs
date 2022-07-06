using Backlang.Codeanalysis.Parsing.AST.Statements;
using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class TypeFieldDeclaration
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        iterator.Match(TokenType.Let);
        return VariableStatement.Parse(iterator, parser);
    }
}