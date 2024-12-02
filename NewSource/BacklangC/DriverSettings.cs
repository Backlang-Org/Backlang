using CommandLine;

namespace BacklangC;

public class DriverSettings
{
    [Option("debug-symbols", Required = false, HelpText = "Enable debug symbols.")]
    public bool DebugSymbols { get; set; }

    [Option("is-debug", Required = false, HelpText = "Specify if in debug mode.")]
    public bool IsDebug { get; set; }

    public bool Optimize => OptimizeLevel != "O0";

    [Value(0, MetaName = "sources", HelpText = "Input source files.", Required = true)]
    [Option('s')]
    public IEnumerable<string> Sources { get; set; } = new List<string>();

    [Option('m')]
    public IEnumerable<string>? MacroReferences { get; set; } = new List<string>();

    [Option("output-path", Default = "compiled.dll", HelpText = "Output path for the compiled file.")]
    public string OutputPath { get; set; }

    [Option("root-namespace", Default = "Compiled", HelpText = "Root namespace for the project.")]
    public string RootNamespace { get; set; }

    [Option("version", Default = "1.0", HelpText = "Version of the project.")]
    public string Version { get; set; }

    [Option('o', "level", Default = "-O0", HelpText = "Optimization level.")]
    public string OptimizeLevel { get; set; }

    [Option('e', "exclude", Separator = ',', HelpText = "Comma-separated list of passes to exclude.")]
    public IEnumerable<string> ExcludedPasses { get; set; } = new List<string>();
}