using Backlang.Driver;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Backlang.NET.Sdk
{
    /// <summary>
    /// Compilation task.
    /// </summary>
    public class BuildTask : Task, ICancelableTask // TODO: ToolTask
    {
        private CancellationTokenSource _cancellation = new CancellationTokenSource();

        /// <summary></summary>
        public string[] Compile { get; set; }

        /// <summary>
        /// Used for debugging purposes.
        /// If enabled a debugger is attached to the current process upon the task execution.
        /// </summary>
        public bool DebuggerAttach { get; set; } = false;

        /// <summary></summary>
        public string DebugType { get; set; }

        /// <summary></summary>
        public string EntryPoint { get; set; }

        /// <summary></summary>
        public bool GenerateFullPaths { get; set; }

        /// <summary></summary>
        public string KeyFile { get; set; }

        public string[] Macros { get; set; }

        /// <summary></summary>
        public string NetFrameworkPath { get; set; }

        /// <summary></summary>
        public string NoWarn { get; set; }

        /// <summary>
        /// Optimization level.
        /// Can be a boolean value (true/false), an integer specifying the level(0-9), or an optimization name (debug, release).</summary>
        public string Optimization { get; set; } = bool.TrueString;

        /// <summary></summary>
        [Required]
        public string OutputName { get; set; }

        /// <summary></summary>
        [Required]
        public string OutputPath { get; set; }

        /// <summary></summary>
        public string OutputType { get; set; }

        /// <summary></summary>
        public string PdbFile { get; set; }

        /// <summary></summary>
        public string PublicSign { get; set; }

        /// <summary></summary>
        public string[] ReferencePath { get; set; }

        /// <summary></summary>
        public ITaskItem[] Resources { get; set; }

        /// <summary></summary>
        public string SourceLink { get; set; }

        /// <summary></summary>
        [Required]
        public string TargetFramework { get; set; }

        /// <summary></summary>
        [Required]
        public string TempOutputPath { get; set; }

        /// <summary></summary>
        public string Version { get; set; }

        /// <summary>
        /// Cancels the task nicely.
        /// </summary>
        public void Cancel()
        {
            _cancellation.Cancel();
        }

        // empty, true, false
        // TODO: embed
        /// <summary></summary>
        public override bool Execute()
        {
            _cancellation = new CancellationTokenSource();

            // initiate our assembly resolver within MSBuild process:
            AssemblyResolver.InitializeSafe();

            if (IsCanceled())
            {
                return false;
            }

            //
            // compose compiler arguments:
            var args = new List<string>(1024)
            {
                "/output-name:" + OutputName,
                "/target:" + (string.IsNullOrEmpty(OutputType) ? "library" : OutputType),
                "/o:" + Optimization,
                "/fullpaths:" + GenerateFullPaths.ToString(),
            };

            if (string.Equals(PublicSign, "true", StringComparison.OrdinalIgnoreCase))
                args.Add("/publicsign+");
            else if (string.Equals(PublicSign, "false", StringComparison.OrdinalIgnoreCase))
                args.Add("/publicsign-");

            AddNoEmpty(args, "target-framework", TargetFramework);
            AddNoEmpty(args, "temp-output", TempOutputPath);
            AddNoEmpty(args, "out", OutputPath);
            AddNoEmpty(args, "m", EntryPoint);
            AddNoEmpty(args, "pdb", PdbFile);
            AddNoEmpty(args, "debug-type", DebugType);// => emitPdb = true
            AddNoEmpty(args, "keyfile", KeyFile);
            AddNoEmpty(args, "v", Version);
            AddNoEmpty(args, "nowarn", NoWarn);
            AddNoEmpty(args, "sourcelink", SourceLink);

            if (ReferencePath != null && ReferencePath.Length != 0)
            {
                foreach (var r in ReferencePath)

                {
                    args.Add("/r:" + r);
                }
            }
            else
            {
                Log.LogWarning("No references specified.");
            }

            if (Resources != null)
            {
                foreach (var res in Resources)
                {
                    args.Add(FormatArgFromItem(res, "res", "LogicalName", "Access"));
                }
            }

            // sources at the end:
            if (Compile != null)
            {
                args.AddRange(Compile.Distinct(StringComparer.InvariantCulture));
            }

            //
            // run the compiler:
            var libs = Environment.GetEnvironmentVariable("LIB") + @";C:\Windows\Microsoft.NET\assembly\GAC_MSIL";

            if (IsCanceled())
            {
                return false;
            }

            if (DebuggerAttach)
            {
                Debugger.Launch();
            }

            // compile
            try
            {
                var context = new CompilerContext();
                CompilerDriver.Compile(context);
                /*var resultCode = PhpCompilerDriver.Run(
                    PhpCommandLineParser.Default,
                    null,
                    args: args.ToArray(),
                    clientDirectory: null,
                    baseDirectory: BasePath,
                    sdkDirectory: NetFrameworkPath,
                    additionalReferenceDirectories: libs,
                    analyzerLoader: new SimpleAnalyzerAssemblyLoader(),
                    output: new LogWriter(this.Log),
                    cancellationToken: _cancellation.Token);
                */

                return !context.Messages.Any();
            }
            catch (Exception ex)
            {
                LogException(ex);
                return false;
            }
        }

        /// <summary>
        /// Gets value indicating user has canceled the task.
        /// </summary>
        public bool IsCanceled()
        {
            return _cancellation != null && _cancellation.IsCancellationRequested;
        }

        private bool AddNoEmpty(List<string> args, string optionName, string optionValue)
        {
            if (string.IsNullOrEmpty(optionValue))
            {
                return false;
            }

            args.Add("/" + optionName + ":" + optionValue);
            return true;
        }

        private string FormatArgFromItem(ITaskItem item, string switchName, params string[] metadataNames)
        {
            var arg = new StringBuilder($"/{switchName}:{item.ItemSpec}");

            foreach (var name in metadataNames)
            {
                var value = item.GetMetadata(name);
                if (string.IsNullOrEmpty(value))
                {
                    // The values are expected in linear order, so we have to end at the first missing one
                    break;
                }

                arg.Append(',');
                arg.Append(value);
            }

            return arg.ToString();
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