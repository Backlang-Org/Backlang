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
        if (type.Qualifier is PointerName pn)
        {
            return new PointerType(ImportType(_assemblyDefinition, pn.ElementName));
        }

        return ImportType(_assemblyDefinition, type.Slice(0, type.PathLength - 1).FullName.ToString(), type.FullyUnqualifiedName.ToString());
    }

    public static TypeReference ImportType(this AssemblyDefinition _assemblyDefinition, string ns, string type)
    {
        foreach (var ar in _assemblyDefinition.MainModule.AssemblyReferences)
        {
            var ass = _assemblyDefinition.MainModule.AssemblyResolver.Resolve(ar, new ReaderParameters() { });

            var tr = new TypeReference(ns, type, ass.MainModule, ass.MainModule);

            if (tr?.Resolve() != null)
            {
                return _assemblyDefinition.MainModule.ImportReference(tr);
            }
        }

        var trr = new TypeReference(ns, type, _assemblyDefinition.MainModule, _assemblyDefinition.MainModule).Resolve();

        return _assemblyDefinition.MainModule.ImportReference(trr);
    }
}