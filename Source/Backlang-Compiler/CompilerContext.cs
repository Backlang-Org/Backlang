using Backlang.Codeanalysis.Parsing.AST;
using Backlang_Compiler.Compiling.Typesystem;
using CommandLine;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang_Compiler;

public sealed class CompilerContext
{
    public IEnumerable<Furesoft.Core.CodeDom.Compiler.Core.IMethod> writeMethods;

    public CompilerContext()
    {
        var corlib = ClrTypeEnvironmentBuilder.BuildAssembly();
        Environment = new Furesoft.Core.CodeDom.Backends.CLR.CorlibTypeEnvironment(corlib);
        Binder = new TypeResolver(corlib);

        var consoleType = Binder.ResolveTypes(new SimpleName("Console").Qualify("System")).FirstOrDefault();

        writeMethods = consoleType.Methods.Where(
            method => method.Name.ToString() == "Write"
                && method.IsStatic
                && method.ReturnParameter.Type == Environment.Void
                && method.Parameters.Count == 1);
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