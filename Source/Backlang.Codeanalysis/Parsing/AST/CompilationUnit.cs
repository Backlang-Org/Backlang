using Loyc.Syntax;

namespace Backlang.Codeanalysis.Parsing.AST;

public sealed class CompilationUnit
{
    public LNodeList Body { get; set; } = new();
    public SourceFile<StreamCharSource> Document { get; internal set; }
    public List<Message> Messages { get; set; } = new();

    public static CompilationUnit FromFile(string filename)
    {
        var document = new SourceDocument(filename);

        return Parser.Parse(document);
    }

    public static CompilationUnit FromText(string text)
    {
        var document = new SourceDocument("inline.back", text);

        return Parser.Parse(document);
    }
}