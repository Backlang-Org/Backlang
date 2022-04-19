namespace Backlang.Codeanalysis.Parsing.AST;

public sealed class CompilationUnit : SyntaxNode
{
    public Block Body { get; set; } = new Block();
    public List<Message> Messages { get; set; } = new List<Message>();

    public static CompilationUnit FromFile(string filename)
    {
        var document = new SourceDocument(filename);

        var result = Parser.Parse(document);

        return (CompilationUnit)result.Tree;
    }

    public static CompilationUnit FromText(string text)
    {
        var document = new SourceDocument("inline.txt", text);

        var result = Parser.Parse(document);

        return (CompilationUnit)result.Tree;
    }
}