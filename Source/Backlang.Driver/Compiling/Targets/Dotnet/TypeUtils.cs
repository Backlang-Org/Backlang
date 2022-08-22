using Mono.Cecil;
using PointerType = Mono.Cecil.PointerType;

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
            var ptrType = ImportType(_assemblyDefinition, pn.ElementName);

            if (pn.Kind == PointerKind.Transient)
            {
                return new PointerType(ptrType);
            }
            else if (pn.Kind == PointerKind.Reference)
            {
                return new ByReferenceType(ptrType);
            }
        }
        else if (type.Qualifier is GenericName gn)
        {
            var innerType = ImportType(_assemblyDefinition, gn.DeclarationName);
            var genericType = new GenericInstanceType(innerType);

            foreach (var arg in gn.TypeArgumentNames)
            {
                genericType.GenericArguments.Add(ImportType(_assemblyDefinition, arg));
            }

            return genericType;
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