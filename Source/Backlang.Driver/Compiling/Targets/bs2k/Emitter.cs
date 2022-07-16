using Furesoft.Core.CodeDom.Compiler.Core;
using System.Text;

namespace Backlang.Driver.Compiling.Targets.bs2k;

public class Emitter
{
    private StringBuilder _builder = new();

    public void EmitFunctionDefinition(IMethod method)
    {
        var signature = NameMangler.Mangle(method);

        if (function_definition->name.location.view() == "main")
        {
            Emit("copy sp, R0", "save current stack pointer into R0 (this is the new stack frame base pointer)");
        }
        result += emit_statement(*program, function_definition->body);
        if (function_definition->name.location.view() == "main")
        {
            emit("halt");
        }
        else
        {
            Emit("copy R0, sp", "clear current stack frame");
            Emit("pop R0", "restore previous stack frame");
            emit("return");
        }
    }

    public override string ToString() => _builder.ToString();

    private void Emit(string instruction, string comment)
    {
        _builder.AppendLine(instruction + $" // {comment}");
    }
}