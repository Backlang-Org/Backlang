using Furesoft.Core.CodeDom.Compiler.Core;
using Mono.Cecil;

namespace Backlang.Driver.Compiling.Targets.Dotnet;

public static class TypeUtils
{
    public static TypeReference ImportType(this AssemblyDefinition _assemblyDefinition, IType type)
    {
        foreach (var ar in _assemblyDefinition.MainModule.AssemblyReferences)
        {
            var tr = new TypeReference(type.FullName.Qualifier.ToString(), type.Name.ToString(),
            _assemblyDefinition.MainModule.AssemblyResolver.Resolve(ar,
                 new ReaderParameters()).MainModule, ar).Resolve();

            if (tr != null)
            {
                return tr;
            }
        }

        return null;
    }
}