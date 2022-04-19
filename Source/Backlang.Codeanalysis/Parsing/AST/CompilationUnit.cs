﻿using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST;

public sealed class CompilationUnit
{
    public LNodeList Body { get; set; }
    public List<Message> Messages { get; set; } = new List<Message>();

    public static CompilationUnit FromFile(string filename)
    {
        var document = new SourceDocument(filename);

        var result = Parser.Parse(document);

        return new CompilationUnit { Body = result.Tree, Messages = result.Messages };
    }

    public static CompilationUnit FromText(string text)
    {
        var document = new SourceDocument("inline.txt", text);

        var result = Parser.Parse(document);

        return new CompilationUnit { Body = result.Tree, Messages = result.Messages };
    }
}