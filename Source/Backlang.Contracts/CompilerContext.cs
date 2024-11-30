using Backlang.Codeanalysis.Core;

namespace Backlang.Contracts;

public sealed class CompilerContext
{
    public ICompilationTarget CompilationTarget;
    public PluginContainer Plugins;

    public DescribedAssembly Assembly { get; set; }

    public PlaygroundData Playground { get; set; } = new();
    public CompilerCliOptions Options { get; set; } = new();

    public Stream OutputStream { get; set; }

    public TypeResolver Binder { get; set; } = new();

    public Scope GlobalScope { get; } = new(null);

    public FileScopeData FileScope { get; set; } = new();

    public List<MethodBodyCompilation> BodyCompilations { get; set; } = new();

    public TypeEnvironment Environment { get; set; }

    public string[] MacroReferences { get; set; }
    public List<Message> Messages { get; set; } = new();

    public string OutputPath { get; set; }

    public string ProjectFile { get; set; }

    public string ResultingOutputPath { get; set; }

    public string TempOutputPath { get; set; }
    public List<CompilationUnit> Trees { get; set; } = new();

    public string CorLib { get; set; }

    public void AddError(LNode node, LocalizableString msg)
    {
        if (node.Range.Source is not SourceFile<StreamCharSource>)
        {
            return;
        }

        Messages.Add(Message.Error(msg, node.Range));
    }
}