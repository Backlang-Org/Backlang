using Backlang.Codeanalysis.Parsing;
using Backlang.Core.Macros;
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
        _macroProcessor = new MacroProcessor(new MessageHolder(), typeof(LeMP.Prelude.BuiltinMacros));

        //_macroProcessor.AddMacros(typeof(StandardMacros).Assembly, false);
        _macroProcessor.AddMacros(typeof(BuiltInMacros).Assembly, false);
        _macroProcessor.PreOpenedNamespaces.Add((Symbol)typeof(BuiltInMacros).Namespace);
    }

    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        foreach (var tree in context.Trees)
        {
            tree.Body = _macroProcessor.ProcessSynchronously(new VList<LNode>(tree.Body));

            var errors = (MessageHolder)_macroProcessor.Sink;
            if (errors.List.Count > 0)
            {
                context.Messages.AddRange(errors.List
                    .Select(_ => Message.Error(tree.Document, _.Formatted, 0, 0)));
            }
        }

        return await next.Invoke(context);
    }
}