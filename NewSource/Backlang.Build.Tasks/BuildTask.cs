using Backlang.Codeanalysis.Parsing;
using Microsoft.Build.Framework;
using BacklangC;
using Task = Microsoft.Build.Utilities.Task;

namespace Backlang.Build.Tasks;

public class BuildTask : Task
{
    [System.ComponentModel.DataAnnotations.Required]
    public ITaskItem[] SourceFiles { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    public string OutputPath { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    public ITaskItem[] ReferencePaths { get; set; }

    public string OptimizeLevel { get; set; }
    public bool DebugSymbols { get; set; }
    public string Configuration { get; set; }
    public string Version { get; set; }
    public string RootNamespace { get; set; }

    public override bool Execute()
    {
        var driver = Driver.Create(new DriverSettings
        {
            OutputPath = OutputPath,
            RootNamespace = RootNamespace,
            Sources = SourceFiles.Select(_ => _.ItemSpec).ToArray(),
            OptimizeLevel = OptimizeLevel,
            DebugSymbols = DebugSymbols,
            IsDebug = Configuration == "Debug",
            Version = Version
        });

        driver.Compile();

        foreach (var message in driver.Messages)
        {
            switch (message.Severity)
            {
                case MessageSeverity.Error:
                    Log.LogError(null, null, null,
                        file: message.Document.FileName, message.Range.Start.Line, message.Range.Start.Column,
                        message.Range.End.Line, message.Range.End.Column, message.Text);
                    break;
                case MessageSeverity.Warning:
                    Log.LogWarning(null, null, null,
                        file: message.Document.FileName, message.Range.Start.Line, message.Range.Start.Column,
                        message.Range.End.Line, message.Range.End.Column, message.Text);
                    break;
            }
        }

        return true;
    }
}