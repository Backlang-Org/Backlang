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
        return ImportType(_assemblyDefinition, type.Slice(0, type.PathLength - 1).FullName.ToString(), type.FullyUnqualifiedName.ToString());
    }

    public static TypeReference ImportType(this AssemblyDefinition _assemblyDefinition, string ns, string type)
    {
        bool isPointer = false;
        if (type.EndsWith(" transient*"))
        {
            type = type.Substring(0, type.Length - " transient*".Length);
            isPointer = true;
        }

        foreach (var ar in _assemblyDefinition.MainModule.AssemblyReferences)
        {
            var ass = _assemblyDefinition.MainModule.AssemblyResolver.Resolve(ar, new ReaderParameters() { });

            var tr = new TypeReference(ns, type, ass.MainModule, ass.MainModule);

            if (tr?.Resolve() != null)
            {
                var rtype = _assemblyDefinition.MainModule.ImportReference(tr);
                if (isPointer)
                {
                    return new PointerType(rtype);
                }

                return rtype;
            }
        }

        var trr = new TypeReference(ns, type, _assemblyDefinition.MainModule, _assemblyDefinition.MainModule).Resolve();

        var rrtype = _assemblyDefinition.MainModule.ImportReference(trr);

        if (isPointer)
        {
            return new PointerType(rrtype);
        }

        return rrtype;
    }
}