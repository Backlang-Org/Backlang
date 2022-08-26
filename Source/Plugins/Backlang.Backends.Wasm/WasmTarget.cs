using Backlang.Contracts;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;
using Furesoft.Core.CodeDom.Compiler.Pipeline;
using LeMP;

namespace Backlang.Backends.Wasm
{
    public class WasmTarget : ICompilationTarget
    {
        public Type IntrinsicType => null;

        public string Name => "wasm";

        public void AfterCompiling(CompilerContext context)
        {
            throw new NotImplementedException();
        }

        public void BeforeCompiling(CompilerContext context)
        {
            context.OutputFilename += ".wasm";
        }

        public void BeforeExpandMacros(MacroProcessor processor)
        {
            throw new NotImplementedException();
        }

        public ITargetAssembly Compile(AssemblyContentDescription contents)
        {
            throw new NotImplementedException();
        }

        public TypeEnvironment Init(CompilerContext context)
        {
            throw new NotImplementedException();
        }

        public void InitReferences(CompilerContext context)
        {
            throw new NotImplementedException();
        }
    }
}