using Backlang.Core;
using Backlang.Driver.Compiling.Typesystem;
using Flo;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace Backlang.Driver.Compiling.Stages;

public sealed class InitTypeSystemStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        context.Binder = new TypeResolver();

        var corLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(uint).Assembly);
        var runtimeLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(ExtensionAttribute).Assembly);
        var consoleLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(Console).Assembly);
        var collectionsLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(BitVector32).Assembly);
        var coreLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(Result<>).Assembly);

        context.Binder.AddAssembly(corLib);
        context.Binder.AddAssembly(coreLib);
        context.Binder.AddAssembly(consoleLib);
        context.Binder.AddAssembly(collectionsLib);
        context.Binder.AddAssembly(runtimeLib);

        ClrTypeEnvironmentBuilder.FillTypes(typeof(uint).Assembly, context.Binder);
        ClrTypeEnvironmentBuilder.FillTypes(typeof(Console).Assembly, context.Binder);
        ClrTypeEnvironmentBuilder.FillTypes(typeof(ExtensionAttribute).Assembly, context.Binder);
        ClrTypeEnvironmentBuilder.FillTypes(typeof(BitVector32).Assembly, context.Binder);
        ClrTypeEnvironmentBuilder.FillTypes(typeof(Result<>).Assembly, context.Binder);

        context.Environment = new Furesoft.Core.CodeDom.Backends.CLR.CorlibTypeEnvironment(corLib);

        var consoleType = context.Binder.ResolveTypes(new SimpleName("Console").Qualify("System")).FirstOrDefault();

        context.writeMethods = consoleType?.Methods.Where(
            method => method.Name.ToString() == "Write"
                && method.IsStatic
                && method.ReturnParameter.Type == context.Environment.Void
                && method.Parameters.Count == 1);

        return await next.Invoke(context);
    }
}