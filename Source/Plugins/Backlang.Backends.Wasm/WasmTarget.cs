using Backlang.Contracts;
using Furesoft.Core.CodeDom.Compiler.Pipeline;
using LeMP;
using System.ComponentModel.Composition;

namespace Backlang.Backends.Wasm
{
    [Export(typeof(ICompilationTarget))]
    public class WasmTarget : ICompilationTarget
    {
        public Type IntrinsicType => null;

        public string Name => "wasm";

        public void AfterCompiling(CompilerContext context)
        {
        }

        public void BeforeCompiling(CompilerContext context)
        {
            context.OutputFilename += ".wasm";
        }

        public void BeforeExpandMacros(MacroProcessor processor)
        {
        }

        public ITargetAssembly Compile(AssemblyContentDescription contents)
        {
            return new WasmAssembly(contents);
        }

        public void InitReferences(CompilerContext context)
        {
        }
    }
}