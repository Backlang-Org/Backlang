using Backlang.Driver;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Backlang.NET.Sdk
{
    public class BuildTask : Task, ICancelableTask
    {
        private CancellationTokenSource _cancellation = new CancellationTokenSource();

        [Required]
        public string[] Compile { get; set; }

        public string DebugType { get; set; }

        public string EntryPoint { get; set; }
        public bool GenerateFullPaths { get; set; }
        public string[] MacroReferences { get; set; }
        public string NetFrameworkPath { get; set; }
        public string NoWarn { get; set; }

        /// <summary>
        /// Optimization level.
        /// Can be a boolean value (true/false), an integer specifying the level(0-9), or an optimization name (debug, release).</summary>
        public string Optimization { get; set; } = bool.TrueString;

        [Required]
        public string OutputName { get; set; }

        [Required]
        public string OutputPath { get; set; }

        public string OutputTree { get; set; }
        public string OutputType { get; set; }
        public string Path { get; set; }
        public string ProjectFile { get; set; }
        public string[] ReferencePath { get; set; }
        public ITaskItem[] Resources { get; set; }
        public string ResultingOutputPath { get; set; }
        public string Target { get; set; }

        [Required]
        public string TargetFramework { get; set; }

        [Required]
        public string TempOutputPath { get; set; }

        public string Version { get; set; }

        public void Cancel()
        {
            _cancellation.Cancel();
        }

        public override bool Execute()
        {
            _cancellation = new CancellationTokenSource();

            // initiate our assembly resolver within MSBuild process:
            AssemblyResolver.InitializeSafe();

            var filename = System.IO.Path.GetFileName(Path);
            Path = Path.Substring(0, Path.Length - filename.Length);

            Compile = Compile.Select(_ => Path + _).ToArray();

            try
            {
                if (Compile == null)
                {
                    Log.LogError("No Source Files specified.");
                    return false;
                }

                var context = new CompilerContext();
                context.InputFiles = Compile;
                context.OutputFilename = OutputName;
                context.OutputType = OutputType;
                context.TempOutputPath = TempOutputPath;
                context.OutputPath = OutputPath;
                context.MacroReferences = MacroReferences;
                context.ResultingOutputPath = ResultingOutputPath;
                context.ProjectFile = ProjectFile;

                if (!string.IsNullOrEmpty(OutputTree))
                {
                    context.OutputTree = bool.Parse(OutputTree);
                }

                CompilerDriver.Compile(context);

                foreach (var msg in context.Messages)
                {
                    Log.LogError(msg.ToString());
                }

                return !context.Messages.Any();
            }
            catch (Exception ex)
            {
                LogException(ex);
                return false;
            }
        }

        public bool IsCanceled()
        {
            return _cancellation != null && _cancellation.IsCancellationRequested;
        }

        private void LogException(Exception ex)
        {
            if (ex is AggregateException aex && aex.InnerExceptions != null)
            {
                foreach (var innerEx in aex.InnerExceptions)
                {
                    LogException(innerEx);
                }
            }
            else
            {
                Log.LogErrorFromException(ex, true);
            }
        }

        // honestly I don't know why msbuild in VS does not handle Console.Output,
        // so we have our custom TextWriter that we pass to Log
        private sealed class LogWriter : TextWriter
        {
            public LogWriter(TaskLoggingHelper log)
            {
                Debug.Assert(log != null);

                Log = log;
                NewLine = "\n";
            }

            public override Encoding Encoding => Encoding.UTF8;
            private StringBuilder Buffer { get; } = new StringBuilder();
            private TaskLoggingHelper Log { get; }

            public override void Write(char value)
            {
                lock (Buffer) // accessed in parallel
                {
                    Buffer.Append(value);
                }

                if (value == '\n')
                {
                    TryLogCompleteMessage();
                }
            }

            public override void Write(string value)
            {
                lock (Buffer)
                {
                    Buffer.Append(value);
                }

                TryLogCompleteMessage();
            }

            private bool LogCompleteMessage(string line)
            {
                // TODO: following logs only Warnings and Errors,
                // to log Info diagnostics properly, parse it by ourselves

                return Log.LogMessageFromText(line.Trim(), MessageImportance.High);
            }

            private bool TryLogCompleteMessage()
            {
                string line = null;

                lock (Buffer)   // accessed in parallel
                {
                    // get line from the buffer:
                    for (var i = 0; i < Buffer.Length; i++)
                    {
                        if (Buffer[i] == '\n')
                        {
                            line = Buffer.ToString(0, i);

                            Buffer.Remove(0, i + 1);
                        }
                    }
                }

                //
                return line != null && LogCompleteMessage(line);
            }
        }
    }
}