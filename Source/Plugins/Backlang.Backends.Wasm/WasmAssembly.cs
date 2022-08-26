using Furesoft.Core.CodeDom.Compiler.Pipeline;
using Wasm;
using Wasm.Optimize;

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
        var file = new WasmFile();

        var typeSection = new TypeSection();
        file.Sections.Add(typeSection);

        var memSection = new MemorySection();
        file.Sections.Add(memSection);

        var codeSection = new CodeSection();
        file.Sections.Add(codeSection);

        file.Optimize();

        file.WriteBinaryTo(output);
    }
}