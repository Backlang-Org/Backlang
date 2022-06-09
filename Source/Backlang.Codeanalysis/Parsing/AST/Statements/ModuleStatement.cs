using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Statements;

public sealed class ModuleDeclaration : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        //module <identifier>
        //module <identifier>.<identifier>

        var tree = SyntaxTree.Module(Expression.Parse(parser));

        iterator.Match(TokenType.Semicolon);

        return tree;
    }
}