using System;
using System.IO;
using System.Reflection;

namespace Backlang.NET.Sdk
{
    internal sealed class AssemblyResolver
    {
        /// <summary>
        /// Build task directory containing our assemblies.
        /// </summary>
        private static readonly string s_path = Path.GetDirectoryName(Path.GetFullPath(typeof(AssemblyResolver).Assembly.Location));

        /// <summary>
        /// Resolve assembly.
        /// </summary>
        private static ResolveEventHandler assemblyresolver = new ResolveEventHandler(AssemblyResolve);

        public static void InitializeSafe()
        {
            try
            {
                var domain = AppDomain.CurrentDomain;

                // re-add the event handler

                domain.AssemblyResolve -= assemblyresolver;
                domain.AssemblyResolve += assemblyresolver;
            }
            catch
            {
            }
        }

        private static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // try to resolve assemblies within our task directory
            // we'll ignore the minor version of the requested assembly
            var assname = new AssemblyName(args.Name);

            try
            {
                var hintpath = Path.Combine(s_path, assname.Name + ".dll");
                if (File.Exists(hintpath))
                {
                    // try to load the assembly:
                    return Assembly.LoadFile(hintpath);
                }
            }
            catch
            {
            }

            return null;
        }
    }
}