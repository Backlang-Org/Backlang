using Backlang.Codeanalysis.Parsing.AST;
using CommandLine;

namespace Backlang_Compiler;

public class CompilerContext
{
    [Option('i', "input", Required = true, HelpText = "Input files to be compiled.")]
    public IEnumerable<string> InputFiles { get; set; }

    [Option('o', "output", Required = true, HelpText = "Output filename")]
    public string OutputFilename { get; set; }

    public List<CompilationUnit> Trees { get; set; } = new();
}