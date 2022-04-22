using Backlang.Driver.Compiling.Typesystem;
using Flo;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using System.Collections.Specialized;

namespace Backlang.Driver.Compiling.Stages;

public sealed class InitTypeSystemStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        context.Binder = new TypeResolver();

        var corLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(uint).Assembly);
        var consoleLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(Console).Assembly);
        var collectionsLib = ClrTypeEnvironmentBuilder.CollectTypes(typeof(BitVector32).Assembly);

        context.Binder.AddAssembly(corLib);
        context.Binder.AddAssembly(consoleLib);
        context.Binder.AddAssembly(collectionsLib);

        ClrTypeEnvironmentBuilder.FillTypes(typeof(uint).Assembly, context.Binder);
        ClrTypeEnvironmentBuilder.FillTypes(typeof(Console).Assembly, context.Binder);
        ClrTypeEnvironmentBuilder.FillTypes(typeof(BitVector32).Assembly, context.Binder);

        context.Environment = new Furesoft.Core.CodeDom.Backends.CLR.CorlibTypeEnvironment(corLib);

        var consoleType = context.Binder.ResolveTypes(new SimpleName("Console").Qualify("System")).FirstOrDefault();

        context.writeMethods = consoleType?.Methods.Where(
            method => method.Name.ToString() == "Write"
                && method.IsStatic
                && method.ReturnParameter.Type == context.Environment.Void
                && method.Parameters.Count == 1);

        context.ExtensionsType = new DescribedType(new SimpleName("Extensions").Qualify("Example"), context.Assembly);

        return await next.Invoke(context);
    }
}