using Furesoft.Core.CodeDom.Compiler.Core;
using Furesoft.Core.CodeDom.Compiler.Core.Names;
using Furesoft.Core.CodeDom.Compiler.Core.TypeSystem;

namespace Backlang.Driver.Compiling.Targets.bs2k;

public static class Demangler
{
    public static IMethod Demangle(string labelname, IAssembly assembly, TypeResolver binder, ref DescribedType parentType)
    {
        var parts = labelname.Split('$');

        var namespaces = parts[0].Split("%", StringSplitOptions.RemoveEmptyEntries);

        if (parentType == null)
        {
            var name = QualifyNamespace(namespaces);

            parentType = new DescribedType(name, assembly);
        }

        var method = new DescribedMethod(parentType, new SimpleName(parts[1]), true, null); //return type cannot be determined in label

        foreach (var pType in parts.Skip(2))
        {
            method.AddParameter(new Parameter(binder.ResolveTypes(new SimpleName(pType).Qualify()).FirstOrDefault())); //ToDo: must be correctly resolved
        }

        return method;
    }

    private static QualifiedName QualifyNamespace(string[] namespaces)
    {
        QualifiedName qualified = new SimpleName("").Qualify();

        for (int i = 0; i < namespaces.Length; i++)
        {
            if (i == 0) qualified = new QualifiedName(namespaces[i]);
            else qualified = qualified.Qualify(namespaces[i]);
        }

        return qualified;
    }
}