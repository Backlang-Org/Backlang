using Backlang.Contracts;
using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Pipeline;
using WebAssembly;
using WebAssembly.Instructions;

namespace Backlang.Backends.Wasm;

public class WasmAssembly : ITargetAssembly
{
    private AssemblyContentDescription contents;

    public WasmAssembly(AssemblyContentDescription contents)
    {
        this.contents = contents;
    }

    public void WriteTo(Stream output)
    {
        var module = new Module();

        var program = contents.Assembly.Types.First(_ => _.FullName.ToString() == $".{Names.ProgramClass}");

        for (var i = 0; i < program.Methods.Count; i++)
        {
            var m = program.Methods[i];
            var type = new WebAssemblyType();

            if (m.ReturnParameter.Type.Name.ToString() != "Void")
            {
                type.Returns = new[] { GetWasmType(m.ReturnParameter.Type) };
            }

            module.Types.Add(type);

            module.Functions.Add(new Function
            {
                Type = 0,
            });

            module.Codes.Add(new FunctionBody(new End()));

            if (m == contents.EntryPoint)
            {
                module.Start = (uint)i;
            }
        }

        module.WriteToBinary(output);
    }

    private WebAssemblyValueType GetWasmType(IType type)
    {
        if (type.Name.ToString() == "Int32")
        {
            return WebAssemblyValueType.Int32;
        }
        else if (type.Name.ToString() == "Int64")
        {
            return WebAssemblyValueType.Int64;
        }

        return WebAssemblyValueType.Int32;
    }
}