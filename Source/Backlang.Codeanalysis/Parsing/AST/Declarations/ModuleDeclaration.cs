﻿using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST.Declarations;

public sealed class ModuleDeclaration : IParsePoint
{
    public static LNode Parse(TokenIterator iterator, Parser parser)
    {
        //module <identifier>
        //module <identifier>.<identifier>
        var keywordToken = iterator.Prev;
        var tree = SyntaxTree.Module(Expression.Parse(parser));

        iterator.Match(TokenType.Semicolon);

        return tree.WithRange(keywordToken, iterator.Prev);
    }
}