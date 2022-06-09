using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class ModuleDeclaration : IParsePoint<LNode>
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        //module <identifier>
        //module <identifier>.<identifier>
        var keywordToken = iterator.Peek(-1);
        var tree = SyntaxTree.Module(Expression.Parse(parser)).WithRange(keywordToken, iterator.Current);

        iterator.Match(TokenType.Semicolon);

        return tree;
    }
}