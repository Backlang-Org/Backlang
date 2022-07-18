using Furesoft.Core.CodeDom.Compiler;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Constants;
using Furesoft.Core.CodeDom.Compiler.Instructions;
using Furesoft.Core.CodeDom.Compiler.TypeSystem;
using System.Reflection;
using System.Text;
using MethodBody = Furesoft.Core.CodeDom.Compiler.MethodBody;

namespace Backlang.Driver.Compiling.Targets.bs2k;

public class Emitter
{
    private readonly IMethod _mainMethod;
    private StringBuilder _builder = new();

    public Emitter(IMethod mainMethod)
    {
        _mainMethod = mainMethod;
    }

    public void EmitFunctionDefinition(DescribedBodyMethod method)
    {
        var signature = NameMangler.Mangle(method);

        Emit($"{signature}:", null, 0);

        if (method == _mainMethod)
        {
            Emit("copy sp, R0", "save current stack pointer into R0 (this is the new stack frame base pointer)");
        }

        Emit("");

        EmitMethodBody(method, method.Body);

        Emit("");

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

    private static bool IsIntrinsicType(CallPrototype callPrototype)
    {
        return callPrototype.Callee.ParentType.FullName.ToString() == typeof(Intrinsics).ToString();
    }

    private static bool MatchesParameters(IMethod m, List<Type> argTypes)
    {
        bool matches = false;
        for (int i = 0; i < m.Parameters.Count; i++)
        {
            if (m.Parameters[i].Type.FullName.ToString() == argTypes[i].FullName.ToString())
            {
                matches = (matches || i == 0) && m.Parameters[i].Type.FullName.ToString() == argTypes[i].FullName.ToString();
            }
        }

        return matches;
    }

    private static MethodInfo GetMatchingIntrinsicMethod(IMethod callee)
    {
        var methods = typeof(Intrinsics).GetMethods().Where(_ => _.IsStatic)
                    .Where(_ => _.Name.Equals(callee.Name.ToString(), StringComparison.InvariantCultureIgnoreCase));

        foreach (var m in methods)
        {
            if (MatchesParameters(callee, m.GetParameters().Select(_ => _.ParameterType).ToList()))
            {
                return m;
            }
        }

        return null;
    }

    //Todo: Fix intrinsic double emit constant
    private void EmitConstant(ConstantPrototype consProto)
    {
        if (consProto.Value is IntegerConstant ic)
        {
            Emit("//push immeditate onto stack");
            Emit($"copy {ic.ToInt32()}, R1");
            Emit("push R1");

            Emit("");
        }
    }

    private object InvokeIntrinsic(IMethod callee, object[] arguments)
    {
        var method = GetMatchingIntrinsicMethod(callee);

        return method.Invoke(null, arguments);
    }

    private void EmitBinary(IntrinsicPrototype arith)
    {
        Emit("pop R2", $"store rhs for {arith.Name}-operator in R1");
        Emit("pop R1", $"store lhs for {arith.Name}-operator in R1");

        switch (arith.Name)
        {
            case "arith.+": Emit("add R1, R2, R3", "push result onto stack"); break;
            case "arith.*": Emit("mult R1, R2, R3, R4", "multiply values"); break;
            case "arith.|": Emit("or R1, R2, R3"); break;
            case "arith.&": Emit("and R1, R2, R3"); break;
            case "arith.^": Emit("xor R1, R2, R3"); break;
            default:
                break;
        }

        Emit("push R3", "push result onto stack");

        Emit("");
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
        Emit("copy sp, R7", "this will be the new stack frame base pointer");

        Emit("", "evaluate arguments in current stack frame");
        /*
        for (const auto&argument : expression.arguments) {
            argument->accept(*this);
        }
        */

        Emit("copy R7, R0", "set the new stack frame base pointer");
        Emit("");

        Emit("add ip, 24, R1", "calculate return address");
        Emit("copy R1, *R6", "fill return address placeholder");
        Emit("jump R5", "call function");

        // after the call the return value is inside R1
        Emit("push R1", "push return value onto stack");
    }

    private object GetValue(Constant value)
    {
        switch (value)
        {
            case StringConstant str:
                return str.Value;

            case Float32Constant f32:
                return f32.Value;

            case Float64Constant f64:
                return f64.Value;

            case NullConstant:
                return null;

            case IntegerConstant ic:
                switch (ic.Spec.Size)
                {
                    case 1:
                        return !ic.IsZero;

                    case 8:
                        if (ic.Spec.IsSigned)
                        {
                            return ic.ToInt8();
                        }
                        else
                        {
                            return ic.ToUInt8();
                        }

                    case 16:
                        if (ic.Spec.IsSigned)
                        {
                            return ic.ToInt16();
                        }
                        else
                        {
                            return ic.ToUInt16();
                        }

                    case 32:
                        if (ic.Spec.IsSigned)
                        {
                            return ic.ToInt32();
                        }
                        else
                        {
                            return ic.ToUInt32();
                        }

                    case 64:
                        if (ic.Spec.IsSigned)
                        {
                            return ic.ToInt64();
                        }
                        else
                        {
                            return ic.ToUInt64();
                        }

                    default:
                        break;
                }

                return null;
        }

        return null;
    }

    private void EmitMethodBody(DescribedBodyMethod method, MethodBody body)
    {
        var firstBlock = body.Implementation.NamedInstructions.First().Block;

        foreach (var item in body.Implementation.NamedInstructions)
        {
            var instruction = item.Instruction;

            if (instruction.Prototype is CallPrototype callPrototype)
            {
                if (IsIntrinsicType(callPrototype))
                {
                    var arguments = new List<object>();

                    foreach (var argTag in instruction.Arguments)
                    {
                        var argPrototype = (ConstantPrototype)body.Implementation.GetInstruction(argTag).Prototype;

                        arguments.Add(GetValue(argPrototype.Value));
                    }

                    var intrinsic = InvokeIntrinsic(callPrototype.Callee, arguments.ToArray()).ToString();

                    foreach (var intr in intrinsic.Split("\n"))
                    {
                        Emit(intr, "// emited from intrinsic");
                    }

                    continue;
                }

                Emit($"// Calling '{callPrototype.Callee.FullName}'");
                EmitCall(instruction, body.Implementation);
            }
            else if (instruction.Prototype is NewObjectPrototype newObjectPrototype)
            {
                //EmitNewObject(newObjectPrototype);
            }
            else if (instruction.Prototype is ConstantPrototype consProto)
            {
                EmitConstant(consProto);
            }
            else if (instruction.Prototype is AllocaPrototype allocA)
            {
                EmitVariableDeclaration(item, allocA);
            }
            else if (instruction.Prototype is IntrinsicPrototype arith)
            {
                EmitBinary(arith);
            }
        }
    }
}