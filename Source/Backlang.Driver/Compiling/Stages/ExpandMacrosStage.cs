using Backlang.Codeanalysis.Parsing;
using Backlang.Core.Macros;
using Backlang.Driver.InternalMacros;
using Flo;
using LeMP;
using Loyc;
using Loyc.Collections;
using Loyc.Syntax;
using System.Runtime.Loader;

namespace Backlang.Driver.Compiling.Stages;

public sealed class ExpandMacrosStage : IHandler<CompilerContext, CompilerContext>
{
    private MacroProcessor _macroProcessor;

    public ExpandMacrosStage()
    {
        _macroProcessor = new MacroProcessor(new MessageHolder(), typeof(LeMP.Prelude.BuiltinMacros));

        //_macroProcessor.AddMacros(typeof(StandardMacros).Assembly, false);
        _macroProcessor.AddMacros(typeof(BuiltInMacros).Assembly, false);
        _macroProcessor.AddMacros(typeof(SyntacticMacros).Assembly, false);
        _macroProcessor.PreOpenedNamespaces.Add((Symbol)typeof(BuiltInMacros).Namespace);
        _macroProcessor.PreOpenedNamespaces.Add((Symbol)typeof(SyntacticMacros).Namespace);
    }

    public async Task<CompilerContext> HandleAsync(CompilerContext context, Func<CompilerContext, Task<CompilerContext>> next)
    {
        if (context.MacroReferences != null)
        {
            var loadContext = new AssemblyLoadContext("Macros");
            foreach (var ml in context.MacroReferences)
            {
                var basePath = new FileInfo(context.ProjectFile).Directory.FullName;
                var directory = new FileInfo(context.ResultingOutputPath).Directory.FullName;
                var assembly = loadContext.LoadFromAssemblyPath(Path.Combine(basePath, directory, ml));

                if (assembly == null)
                {
                    context.Messages.Add(Message.Error(null, "Could not load " + ml, -1, -1));
                }
                else
                {
                    _macroProcessor.AddMacros(assembly, false);
                }
            }
        }

        _macroProcessor.DefaultScopedProperties.Add("Target", context.Target);

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