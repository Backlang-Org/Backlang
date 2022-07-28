using Flo;

namespace Backlang.Driver.Compiling.Stages;

public sealed class PluginSystemStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        var plugins = PluginContainer.Load();
        if (plugins == null)
        {
            return await next.Invoke(context);
        }

        context.Plugins = plugins;

        return await next.Invoke(context);
    }
}