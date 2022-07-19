using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Constants;
using Furesoft.Core.CodeDom.Compiler.Instructions;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using System.Text;

namespace Backlang.Driver.Compiling.Targets.bs2k;

public class Emitter
{
    private readonly IMethod _mainMethod;
    private StringBuilder _builder = new();

    public Emitter(IMethod mainMethod)
    {
        this._mainMethod = mainMethod;
    }

    public void EmitFunctionDefinition(DescribedBodyMethod method)
    {
        var signature = NameMangler.Mangle(method);

        Emit($"{signature}:", null, 0);

        if (method == _mainMethod)
        {
            Emit("copy sp, R0", "save current stack pointer into R0 (this is the new stack frame base pointer)");
        }

        EmitMethodBody(method, method.Body);

        if (method == _mainMethod)
        {
            Emit("halt");
        }
        else
        {
            Emit("copy R0, sp", "clear current stack frame");
            Emit("pop R0", "restore previous stack frame");
            Emit("return");
        }
    }

    public override string ToString() => _builder.ToString();

    public void Emit(string instruction, string comment = null, int indentlevel = 1)
    {
        if (indentlevel == 1)
        {
            _builder.Append('\t');
        }

        if (comment == null)
        {
            _builder.AppendLine(instruction);
            return;
        }

        _builder.AppendLine($"{instruction} // {comment}");
    }

    private void EmitMethodBody(DescribedBodyMethod method, MethodBody body)
    {
        var firstBlock = body.Implementation.NamedInstructions.First().Block;

        for (int i = 0; i < firstBlock.Parameters.Count; i++)
        {
            var varname = firstBlock.Parameters[i].Tag.Name;

            //ToDo: Variables must be rewritten
            var namedInstructions = body.Implementation.NamedInstructions.ToArray();
            for (var i1 = 0; i1 < namedInstructions.Length; i1++)
            {
                var item = namedInstructions[i1];
                var instruction = item.Instruction;

                if (instruction.Prototype is AllocaPrototype allocA)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (instruction.Prototype is not ConstantPrototype && item.NextInstructionOrNull is not null)
                        {
                            instruction = item.NextInstructionOrNull.Instruction;
                        }
                    }

                    if (instruction.Prototype is ConstantPrototype constantPrototype)
                    {
                        EmitImmediate(constantPrototype.Value);
                    }
                }
            }
        }

        foreach (var item in body.Implementation.NamedInstructions)
        {
            var instruction = item.Instruction;

            if (instruction.Prototype is CallPrototype callPrototype)
            {
                EmitCall(instruction, body.Implementation);
            }
            else if (instruction.Prototype is NewObjectPrototype newObjectPrototype)
            {
                //EmitNewObject(newObjectPrototype);
            }
            else if (instruction.Prototype is LoadPrototype ld)
            {
                var consProto = (ConstantPrototype)item.PreviousInstructionOrNull.Prototype;

                //EmitConstant(consProto);
            }
            else if (instruction.Prototype is AllocaPrototype allocA)
            {
                EmitVariableDeclaration(item, allocA);
            }
        }
    }

    private void EmitImmediate(Constant value)
    {
        if (value is IntegerConstant icons)
        {
            Emit($"copy {icons.ToInt64().ToString()}, R1", "// put immediate into register");
            Emit("push R1", "// push immediate onto stack");
        }
    }

    private void EmitVariableDeclaration(NamedInstruction item, AllocaPrototype allocA)
    {
    }

    private void EmitCall(Instruction instruction, FlowGraph implementation)
    {
        var prototype = (CallPrototype)instruction.Prototype;

        Emit($"copy {NameMangler.Mangle(prototype.Callee)}, R1", "get address of label");
        Emit("push R1", "push address of label onto stack");
        Emit("pop R5", "get jump address");

        Emit("copy sp, R6", "store address of return address placeholder");
        Emit("add sp, 4, sp", "reserve stack space for the return address placeholder");

        Emit("push R0", "store current stack frame base pointer for later");
        Emit("copy sp, R0");

        /*
        for (const auto&argument : expression.arguments) {
            argument->accept(*this);
        }
        */

        Emit("add ip, 24, R1", "calculate return address");
        Emit("copy R1, *R6", "fill return address placeholder");
        Emit("jump R5", "call function");

        // after the call the return value is inside R1
        Emit("push R1", "push return value onto stack");
    }
}