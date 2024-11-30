using Backlang.Contracts;
using Backlang.Driver;
using System;
using System.IO;
using System.Runtime.InteropServices.JavaScript;
using System.Text;

public partial class Bridge
{
    [JSExport]
    public static string CompileAndRun(string src)
    {
        var context = new CompilerContext();
        context.Playground = new PlaygroundData { IsPlayground = true, Source = src };

        var assemblyStream = new MemoryStream();

        context.OutputStream = assemblyStream;

        var output = new MemoryStream();
        var sw = new StreamWriter(output);
        Console.SetOut(sw);

        CompilerDriver.Compile(context);

        assemblyStream.Seek(0, SeekOrigin.Begin);

        //var assembly = Assembly.Load(assemblyStream.ToArray());
        //assembly.EntryPoint.Invoke(null, Array.Empty<object>());

        return Encoding.UTF8.GetString(assemblyStream.ToArray());
    }
}