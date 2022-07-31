using Furesoft.Core.CodeDom.Compiler.Pipeline;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;

namespace Backlang.Driver.Compiling.Targets.bs2k;

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

        if (!Contents.Assembly.IsLibrary)
        {
            emitter.Emit($"jump {NameMangler.Mangle(Contents.EntryPoint)}", "call main method\n", 0);
        }

        var program = Contents.Assembly.Types.First(_ => _.FullName.ToString() == $".{Names.ProgramClass}");

        emitter.EmitStringConstants(program);

        foreach (var method in program.Methods)
        {
            if (method is DescribedBodyMethod m)
            {
                emitter.EmitFunctionDefinition(m);
            }
        }
;
        var sw = new StreamWriter(output);
        sw.Write(emitter.ToString());

        sw.Close();
    }
}