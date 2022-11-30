using Backlang.Driver;
using Backlang.Codeanalysis.Core;

namespace Backlang.Contracts;

public class NamespaceImports
{
    public readonly List<QualifiedName> ImportedNamespaces = new();

    public void ImportNamespace(LNode importStatement, CompilerContext context)
    {
        if (importStatement is ("#import", var ns))
        {
            var qualifiedNs = ConversionUtils.GetQualifiedName(ns);

            if (ImportedNamespaces.Contains(qualifiedNs))
            {
                context.AddError(importStatement, new(ErrorID.NamespaceAlreadyImported, qualifiedNs.ToString()));
                return;
            }

            ImportedNamespaces.Add(qualifiedNs);

            ExpandNamespaceImports(this, context);
        }
    }

    private static void ExpandNamespaceImports(NamespaceImports ns, CompilerContext context)
    {
        for (var i = 0; i < ns.ImportedNamespaces.Count; i++)
        {
            var import = ns.ImportedNamespaces[i];
            var imp = import.ToString();

            if (!imp.EndsWith(".*"))
            {
                continue;
            }

            var withoutWildcard = imp[..^2];

            context.Binder.TryResolveNamespace(ConversionUtils.QualifyNamespace(withoutWildcard), out var foundNamespaces);

            if (foundNamespaces == null)
            {
                //ToDo: add error that namespace has no subnamespace(s)
                return;
            }

            ns.ImportedNamespaces.Remove(import);
            foreach (var foundNs in foundNamespaces.Namespaces)
            {
                ns.ImportedNamespaces.Add(foundNs.Key.Qualify(withoutWildcard));
            }
        }
    }
}
