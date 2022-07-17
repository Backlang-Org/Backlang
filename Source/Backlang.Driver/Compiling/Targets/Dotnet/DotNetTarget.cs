using Backlang.Core;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Pipeline;
using LeMP;
using Loyc.Syntax;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace Backlang.Driver.Compiling.Targets.Dotnet;

public class DotNetTarget : ICompilationTarget
{
    public string Name => "dotnet";

    public bool HasIntrinsics => false;

    public Type IntrinsicType => throw new NotImplementedException();

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
    }

    public void BeforeExpandMacros(MacroProcessor processor)
    {
    }

    public ITargetAssembly Compile(AssemblyContentDescription contents)
    {
        return new DotNetAssembly(contents);
    }

    public LNode ConvertIntrinsic(LNode calls)
    {
        throw new NotImplementedException();
    }

    public TypeEnvironment Init(TypeResolver binder)
    {
        var corLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(uint).Assembly);
        var runtimeLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(ExtensionAttribute).Assembly);
        var consoleLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(Console).Assembly);
        var collectionsLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(BitVector32).Assembly);
        var coreLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(Result<>).Assembly);

        binder.AddAssembly(corLib);
        binder.AddAssembly(coreLib);
        binder.AddAssembly(consoleLib);
        binder.AddAssembly(collectionsLib);
        binder.AddAssembly(runtimeLib);

        ClrTypeEnvironmentBuilder.FillTypes(typeof(uint).Assembly, binder);
        ClrTypeEnvironmentBuilder.FillTypes(typeof(Console).Assembly, binder);
        ClrTypeEnvironmentBuilder.FillTypes(typeof(ExtensionAttribute).Assembly, binder);
        ClrTypeEnvironmentBuilder.FillTypes(typeof(BitVector32).Assembly, binder);
        ClrTypeEnvironmentBuilder.FillTypes(typeof(Result<>).Assembly, binder);

        return new Furesoft.Core.CodeDom.Backends.CLR.CorlibTypeEnvironment(corLib);
    }
}