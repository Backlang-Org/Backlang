using Backlang.Codeanalysis.Parsing;
using Backlang.Codeanalysis.Parsing.AST;
using CommandLine;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Driver;

public sealed class CompilerContext
{
    public IEnumerable<Furesoft.Core.CodeDom.Compiler.Core.IMethod> writeMethods;

    public DescribedAssembly Assembly { get; set; }

    public TypeResolver Binder { get; set; } = new();
    public TypeEnvironment Environment { get; set; }
    public DescribedType ExtensionsType { get; set; }

    [Option('i', "input", Required = true, HelpText = "Input files to be compiled.")]
    public IEnumerable<string> InputFiles { get; set; }

    public List<Message> Messages { get; set; } = new();

    [Option('o', "output", Required = true, HelpText = "Output filename")]
    public string OutputFilename { get; set; }

    [Option('r', "reference", Required = false, HelpText = "References of the assembly")]
    public IEnumerable<string> References { get; set; }

    public List<CompilationUnit> Trees { get; set; } = new();
}