using Flo;

namespace Backlang.Driver.Compiling.Stages.InitStages;

public sealed partial class InitStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        InitPlugins(context);

        InitTypeSystem(context);

        InitReferences(context);

        return await next.Invoke(context);
    }

    private static void InitReferences(CompilerContext context)
    {
        context.CompilationTarget?.InitReferences(context);
    }

    private static void InitPlugins(CompilerContext context)
    {
        var plugins = PluginContainer.Load();
        context.Plugins = plugins;
    }
}