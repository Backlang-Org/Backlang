using Flo;

namespace Backlang.Driver.Compiling.Stages.InitStages;

public sealed class InitEmbeddedResourcesStage : IHandler<CompilerContext, CompilerContext>
{
    public async Task<CompilerContext> HandleAsync(CompilerContext context,
        Func<CompilerContext, Task<CompilerContext>> next)
    {
        foreach (var resource in context.Options.EmbeddedResource)
        {
            Stream strm = File.OpenRead(resource);

            foreach (var preprocessor in context.Plugins?.Preprocessors)
            {
                if (preprocessor.Extension == Path.GetExtension(resource))
                {
                    strm = preprocessor.Preprocess(strm);
                }
            }

            var attr = new EmbeddedResourceAttribute(Path.GetFileName(resource), strm);
            context.Assembly.AddAttribute(attr);
        }

        return await next.Invoke(context);
    }
}