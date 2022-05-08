using Flo;
using LeMP;
using Loyc;
using Loyc.Collections;
using Loyc.Syntax;

namespace Backlang.Driver.Compiling.Stages;

public sealed class ExpandMacrosStage : IHandler<CompilerContext, CompilerContext>
{
    private MacroProcessor _macroProcessor;

    public ExpandMacrosStage()
    {
        _macroProcessor = new MacroProcessor(new NullMessageSink(), typeof(LeMP.Prelude.BuiltinMacros));

        _macroProcessor.AddMacros(typeof(StandardMacros).Assembly, false);
    }

    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        foreach (var tree in context.Trees)
        {
            tree.Body = _macroProcessor.ProcessSynchronously(new VList<LNode>(tree.Body));
        }

        return await next.Invoke(context);
    }
}