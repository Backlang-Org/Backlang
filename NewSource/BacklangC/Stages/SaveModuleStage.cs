using Flo;

namespace BacklangC.Stages;

public sealed class SaveModuleStage : IHandler<Driver, Driver>
{
    public async Task<Driver> HandleAsync(Driver context, Func<Driver, Task<Driver>> next)
    {
        context.Compilation.Module.Save(context.Settings.OutputPath, context.Settings.DebugSymbols);

        return await next.Invoke(context);
    }
}