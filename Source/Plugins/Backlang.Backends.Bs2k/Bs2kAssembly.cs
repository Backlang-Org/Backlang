using Backlang.Contracts;
using Backlang.Driver.Compiling;
using Furesoft.Core.CodeDom.Compiler.Pipeline;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;

namespace Backlang.Backends.Bs2k;

public class Bs2kAssembly : ITargetAssembly
{
    public Bs2kAssembly(AssemblyContentDescription contents)
    {
        Contents = contents;
    }

    public AssemblyContentDescription Contents { get; }

    public void WriteTo(Stream output)
    {
        var emitter = new Emitter(Contents.EntryPoint);

        var program = Contents.Assembly.Types.First(_ => _.FullName.ToString() == $".{Names.ProgramClass}");

        foreach (var er in Contents.Assembly.Attributes.GetAll().Where(_ => _ is EmbeddedResourceAttribute).Cast<EmbeddedResourceAttribute>())
        {
            var rawStrm = new MemoryStream();
            er.Strm.CopyTo(rawStrm);
            rawStrm.Seek(0, SeekOrigin.Begin);

            emitter.EmitResource(er.Name.Replace('.', '$').Replace('/', '$'), rawStrm.ToArray());
        }

        emitter.EmitStringConstants(program);

        if (!Contents.Assembly.IsLibrary)
        {
            emitter.Emit($"jump {NameMangler.Mangle(Contents.EntryPoint)}", "call main method\n", 0);
        }

        foreach (var method in program.Methods)
        {
            if (method is DescribedBodyMethod m)
            {
                emitter.EmitFunctionDefinition(m);
            }
        }

        var sw = new StreamWriter(output);
        sw.Write(emitter.ToString());

        sw.Close();
    }
}