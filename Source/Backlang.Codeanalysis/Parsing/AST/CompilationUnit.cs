using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST;

public sealed class CompilationUnit
{
    public LNodeList Body { get; set; } = new();
    public SourceFile<StreamCharSource> Document { get; private set; }
    public List<Message> Messages { get; set; } = new List<Message>();

    public static CompilationUnit FromFile(string filename)
    {
        var document = new SourceDocument(filename);
        var result = Parser.Parse(document);

        return new CompilationUnit { Body = result.Tree, Messages = result.Messages, Document = document };
    }

    public static CompilationUnit FromText(string text)
    {
        var document = new SourceDocument("inline.txt", text);

        var result = Parser.Parse(document);

        return new CompilationUnit { Body = result.Tree, Messages = result.Messages, Document = document };
    }
}