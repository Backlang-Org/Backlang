using Backlang.Core;
using Furesoft.Core.CodeDom.Compiler.Pipeline;
using LeMP;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Backlang.Driver.Compiling.Targets.Dotnet;

public class DotNetTarget : ICompilationTarget
{
    public string Name => "dotnet";

    public bool HasIntrinsics => true;

    public Type IntrinsicType => typeof(Intrinsics);

    public void AfterCompiling(CompilerContext context)
    {
        var runtimeConfigStream = typeof(DotNetTarget).Assembly.GetManifestResourceStream("Backlang.Driver.compilation.runtimeconfig.json");
        var jsonStream = File.OpenWrite($"{Path.Combine(context.TempOutputPath, Path.GetFileNameWithoutExtension(context.OutputFilename))}.runtimeconfig.json");

        runtimeConfigStream.CopyTo(jsonStream);

        jsonStream.Close();
        runtimeConfigStream.Close();
    }

    public void BeforeCompiling(CompilerContext context)
    {
        context.OutputFilename += ".dll";
    }

    public void BeforeExpandMacros(MacroProcessor processor)
    {
    }

    public ITargetAssembly Compile(AssemblyContentDescription contents)
    {
        return new DotNetAssembly(contents);
    }

    public TypeEnvironment Init(CompilerContext context)
    {
        var corLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(uint).Assembly);
        var runtimeLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(ExtensionAttribute).Assembly);
        var consoleLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(Console).Assembly);
        var collectionsSpecializedLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(BitVector32).Assembly);
        var coreLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(Result<>).Assembly);
        var collectionsLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(ArrayList).Assembly);

        context.Binder.AddAssembly(corLib);
        context.Binder.AddAssembly(coreLib);
        context.Binder.AddAssembly(consoleLib);
        context.Binder.AddAssembly(collectionsSpecializedLib);
        context.Binder.AddAssembly(collectionsLib);
        context.Binder.AddAssembly(runtimeLib);

        ClrTypeEnvironmentBuilder.FillTypes(typeof(uint).Assembly, context);
        ClrTypeEnvironmentBuilder.FillTypes(typeof(Console).Assembly, context);
        ClrTypeEnvironmentBuilder.FillTypes(typeof(ExtensionAttribute).Assembly, context);
        ClrTypeEnvironmentBuilder.FillTypes(typeof(BitVector32).Assembly, context);
        ClrTypeEnvironmentBuilder.FillTypes(typeof(Result<>).Assembly, context);
        ClrTypeEnvironmentBuilder.FillTypes(typeof(ArrayList).Assembly, context);

        return new Furesoft.Core.CodeDom.Backends.CLR.CorlibTypeEnvironment(corLib);
    }

    public void InitReferences(CompilerContext context)
    {
        foreach (var r in context.References)
        {
            var assembly = Assembly.LoadFrom(r);

            var refLib = ClrTypeEnvironmentBuilder.CollectTypes(assembly);

            context.Binder.AddAssembly(refLib);

            ClrTypeEnvironmentBuilder.FillTypes(assembly, context);
        }
    }
}