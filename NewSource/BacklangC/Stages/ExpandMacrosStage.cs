using System.Runtime.Loader;
using Backlang.Codeanalysis.Parsing;
using Backlang.Core.Macros;
using BacklangC.Core;
using Flo;
using LeMP;
using LeMP.Prelude;
using Loyc;
using Loyc.Collections;
using Loyc.Syntax;

namespace BacklangC.Stages;

public sealed class ExpandMacrosStage : IHandler<Driver, Driver>
{
    private readonly MacroProcessor _macroProcessor;

    public ExpandMacrosStage()
    {
        _macroProcessor = new MacroProcessor(new MessageHolder(), typeof(BuiltinMacros));

        //_macroProcessor.AddMacros(typeof(StandardMacros).Assembly, false);
        _macroProcessor.AddMacros(typeof(BuiltInMacros).Assembly, false);
        _macroProcessor.AddMacros(typeof(SyntacticMacros).Assembly, false);
        _macroProcessor.PreOpenedNamespaces.Add((Symbol)typeof(BuiltInMacros).Namespace);
        _macroProcessor.PreOpenedNamespaces.Add((Symbol)typeof(SyntacticMacros).Namespace);
    }

    public async Task<Driver> HandleAsync(Driver context,
        Func<Driver, Task<Driver>> next)
    {
        if (context.Settings.MacroReferences != null)
        {
            var loadContext = new AssemblyLoadContext("Macros");
            foreach (var ml in context.Settings.MacroReferences)
            {
                var directory = new FileInfo(context.Settings.OutputPath).Directory?.FullName;
                var assembly = loadContext.LoadFromAssemblyPath(Path.Combine(directory, ml));

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

    private MessageSeverity ConvertSeverity(Severity severity)
    {
        return severity switch
        {
            Severity.Info => MessageSeverity.Info,
            Severity.Warning => MessageSeverity.Warning,
            Severity.Error => MessageSeverity.Error,
            _ => MessageSeverity.Error
        };
    }
}