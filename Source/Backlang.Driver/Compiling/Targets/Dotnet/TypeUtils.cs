using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Mono.Cecil;

namespace Backlang.Driver.Compiling.Targets.Dotnet;

public static class TypeUtils
{
    public static TypeReference ImportType(this AssemblyDefinition _assemblyDefinition, IType type)
    {
        return ImportType(_assemblyDefinition, type.FullName);
    }

    public static TypeReference ImportType(this AssemblyDefinition _assemblyDefinition, QualifiedName type)
    {
        return ImportType(_assemblyDefinition, type.Qualifier.ToString(), type.Name.ToString());
    }

    public static TypeReference ImportType(this AssemblyDefinition _assemblyDefinition, string ns, string type)
    {
        foreach (var ar in _assemblyDefinition.MainModule.AssemblyReferences)
        {
            var ass = _assemblyDefinition.MainModule.AssemblyResolver.Resolve(ar, new ReaderParameters() { });

            var tr = new TypeReference(ns, type, ass.MainModule, ass.MainModule).Resolve();

            if (tr?.Resolve() != null)
            {
                return _assemblyDefinition.MainModule.ImportReference(tr);
            }
        }

        return _assemblyDefinition.MainModule.ImportReference(
            new TypeReference(ns, type, _assemblyDefinition.MainModule, _assemblyDefinition.MainModule).Resolve());
    }
}