using Backlang.Codeanalysis.Parsing.AST;
using Backlang_Compiler.Compiling.Typesystem;
using CommandLine;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang_Compiler;

public sealed class CompilerContext
{
    public CompilerContext()
    {
        var corlib = ClrTypeEnvironmentBuilder.BuildAssembly();
        Environment = new Furesoft.Core.CodeDom.Backends.CLR.CorlibTypeEnvironment(corlib);
        Binder = new TypeResolver(corlib);

        var consoleType = Binder.ResolveTypes(new SimpleName("Console").Qualify("System")).FirstOrDefault();

        var writeMethod = consoleType.Methods.FirstOrDefault(
            method => method.Name.ToString() == "Write"
                && method.IsStatic
                && method.ReturnParameter.Type == Environment.Void
                && method.Parameters.Count == 1
                && method.Parameters[0].Type == Environment.String);
    }

    public DescribedAssembly Assembly { get; set; }

    public TypeResolver Binder { get; set; } = new();
    public TypeEnvironment Environment { get; set; }

    [Option('i', "input", Required = true, HelpText = "Input files to be compiled.")]
    public IEnumerable<string> InputFiles { get; set; }

    [Option('o', "output", Required = true, HelpText = "Output filename")]
    public string OutputFilename { get; set; }

    public List<CompilationUnit> Trees { get; set; } = new();
}