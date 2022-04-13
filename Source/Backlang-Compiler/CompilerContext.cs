using Backlang.Codeanalysis.Parsing.AST;
using CommandLine;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang_Compiler;

public sealed class CompilerContext
{
    public DescribedAssembly Assembly { get; set; }

    public TypeEnvironment Environment { get; set; }

    [Option('i', "input", Required = true, HelpText = "Input files to be compiled.")]
    public IEnumerable<string> InputFiles { get; set; }

    [Option('o', "output", Required = true, HelpText = "Output filename")]
    public string OutputFilename { get; set; }

    public List<CompilationUnit> Trees { get; set; } = new();
}