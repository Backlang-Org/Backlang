using Backlang.Core;
using Backlang.Driver.Compiling.Targets.Dotnet.RuntimeOptionsModels;
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
        RuntimeConfig.Save(context.TempOutputPath,
            Path.GetFileNameWithoutExtension(context.Options.OutputFilename), context.Options);
    }

    public void BeforeCompiling(CompilerContext context)
    {
        context.Options.OutputFilename += ".dll";
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
        var mscoreLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(uint).Assembly);
        var runtimeLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(ExtensionAttribute).Assembly);
        var consoleLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(Console).Assembly);
        var collectionsSpecializedLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(BitVector32).Assembly);
        var coreLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(Result<>).Assembly);
        var collectionsLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(ArrayList).Assembly);

        context.Binder.AddAssembly(mscoreLib);
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

        return new Furesoft.Core.CodeDom.Backends.CLR.CorlibTypeEnvironment(mscoreLib);
    }

    public void InitReferences(CompilerContext context)
    {
        foreach (var r in context.Options.References)
        {
            AddFromAssembly(context, r);
        }

        if (context.CorLib == null)
        {
            return;
        }

        foreach (var r in context.CorLib.Split(';'))
        {
            AddFromAssembly(context, r);
        }

        static void AddFromAssembly(CompilerContext context, string r)
        {
            var assembly = Assembly.LoadFrom(r);

            var refLib = ClrTypeEnvironmentBuilder.CollectTypes(assembly);

            context.Binder.AddAssembly(refLib);

            ClrTypeEnvironmentBuilder.FillTypes(assembly, context);
        }
    }
}