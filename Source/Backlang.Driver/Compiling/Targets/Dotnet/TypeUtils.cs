using Mono.Cecil;
using Mono.Cecil.Rocks;
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

            if (pn.Kind == PointerKind.Reference)
            {
                return new ByReferenceType(ptrType);
            }
        }
        else if (type.Qualifier is GenericName gn)
        {
            if (gn.DeclarationName.ToString().StartsWith("array!"))
            {
                return ImportArrayType(_assemblyDefinition, gn);
            }

            return ImportGenericType(_assemblyDefinition, gn);
        }

        return ImportType(_assemblyDefinition, type.Slice(0, type.PathLength - 1).FullName,
            type.FullyUnqualifiedName.ToString());
    }

    public static TypeReference ImportType(this AssemblyDefinition _assemblyDefinition, string ns, string type)
    {
        foreach (var ar in _assemblyDefinition.MainModule.AssemblyReferences)
        {
            var ass = _assemblyDefinition.MainModule.AssemblyResolver.Resolve(ar, new ReaderParameters());

            var tr = new TypeReference(ns, type, ass.MainModule, ass.MainModule);

            if (tr?.Resolve() != null)
            {
                return _assemblyDefinition.MainModule.ImportReference(tr);
            }
        }

        var trr = new TypeReference(ns, type, _assemblyDefinition.MainModule, _assemblyDefinition.MainModule).Resolve();

        return _assemblyDefinition.MainModule.ImportReference(trr);
    }

    private static TypeReference ImportArrayType(AssemblyDefinition _assemblyDefinition, GenericName gn)
    {
        var declName = gn.DeclarationName.ToString();
        var rankStr = declName.Substring(declName.IndexOf("!") + 1,
            declName.IndexOf("`") - (declName.LastIndexOf("!") + 1));
        var rank = int.Parse(rankStr);

        var elType = _assemblyDefinition.ImportType(gn.TypeArgumentNames[0]);
        return elType.MakeArrayType(rank);
    }

    private static TypeReference ImportGenericType(AssemblyDefinition _assemblyDefinition, GenericName gn)
    {
        var innerType = ImportType(_assemblyDefinition, gn.DeclarationName);
        var genericType = new GenericInstanceType(innerType);

        foreach (var arg in gn.TypeArgumentNames)
        {
            genericType.GenericArguments.Add(ImportType(_assemblyDefinition, arg));
        }

        return genericType;
    }
}