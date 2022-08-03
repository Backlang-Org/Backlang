using Backlang.Contracts;
using Flo;

namespace Backlang.Driver.Compiling.Stages.InitStages;

public sealed class InitEmbeddedResourcesStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        foreach (var resource in context.EmbeddedResource)
        {
            var attr = new EmbeddedResourceAttribute(Path.GetFileName(resource), resource);
            context.Assembly.AddAttribute(attr);
        }

        return await next.Invoke(context);
    }
}