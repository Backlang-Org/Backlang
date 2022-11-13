namespace Backlang.Contracts;

public class CompilerCliOptions
{
    public CompilerCliOptions()
    {
        References = Array.Empty<string>();
        EmbeddedResource = Array.Empty<string>();
    }

    [Option('i', "input", Required = true, HelpText = "Input files to be compiled.")]
    public IEnumerable<string> InputFiles { get; set; }

    [Option('o', "output", Required = true, HelpText = "Output filename")]
    public string OutputFilename { get; set; }

    [Option('p', "print-tree", Required = false, HelpText = "Output files as tree")]
    public bool OutputTree { get; set; }

    [Option('t', "type", Required = false, HelpText = "Outputtype")]
    public string OutputType { get; set; }

    [Option('r', "reference", Required = false, HelpText = "References of the assembly")]
    public IEnumerable<string> References { get; set; }

    [Option("target", Required = false, HelpText = "For which platform to compile to")]
    public string Target { get; set; }

    [Option('e', longName: "embedd", HelpText = "Embedd files into the assembly as resource")]
    public IEnumerable<string> EmbeddedResource { get; set; }

    [Option('v', longName: "version", HelpText = "Set the assembly version")]
    public string Version { get; set; }
}