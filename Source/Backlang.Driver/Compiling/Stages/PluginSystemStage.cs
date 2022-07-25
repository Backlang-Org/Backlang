using Flo;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace Backlang.Driver.Compiling.Stages;

public sealed class PluginSystemStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        var catalog = new DirectoryCatalog("."); //ToDo: Change Plugin Directory
        var container = new CompositionContainer(catalog);

        var plugins = new PluginContainer();
        container.ComposeParts(plugins);

        context.Plugins = plugins;

        return await next.Invoke(context);
    }
}