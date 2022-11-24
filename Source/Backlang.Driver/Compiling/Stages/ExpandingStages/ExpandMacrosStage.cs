using Backlang.Core.Macros;
using Backlang.Driver.InternalMacros;
using Flo;
using LeMP;
using Loyc.Collections;
using System.Runtime.Loader;

namespace Backlang.Driver.Compiling.Stages.ExpandingStages;

public sealed class ExpandMacrosStage : IHandler<CompilerContext, CompilerContext>
{
    private readonly MacroProcessor _macroProcessor;

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
        context.CompilationTarget.BeforeExpandMacros(_macroProcessor); //Only calls once

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
                    context.Messages.Add(Message.Error("Could not load " + ml, SourceRange.Synthetic));
                }
                else
                {
                    _macroProcessor.AddMacros(assembly, false);
                }
            }
        }

        _macroProcessor.DefaultScopedProperties.Add("Target", context.Options.Target);
        _macroProcessor.DefaultScopedProperties.Add("Context", context);

        foreach (var tree in context.Trees)
        {
            tree.Body = _macroProcessor.ProcessSynchronously(new VList<LNode>(tree.Body));

            var errors = (MessageHolder)_macroProcessor.Sink;
            if (errors.List.Count > 0)
            {
                foreach (var error in errors.List)
                {
                    var range = (SourceRange)error.Location;

                    var msg = Message.Error(error.Formatted, range);
                    msg.Severity = ConvertSeverity(error.Severity);

                    context.Messages.Add(msg);
                }
            }
        }

        return await next.Invoke(context);
    }

    private static MessageSeverity ConvertSeverity(Severity severity)
    {
        return severity switch
        {
            Severity.Info => MessageSeverity.Info,
            Severity.Warning => MessageSeverity.Warning,
            Severity.Error => MessageSeverity.Error,
            _ => MessageSeverity.Error,
        };
    }
}