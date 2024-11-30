using Backlang.Codeanalysis.Core;
using Backlang.Driver;

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
                context.AddError(importStatement,
                    new LocalizableString(ErrorID.NamespaceAlreadyImported, qualifiedNs.ToString()));
                return;
            }

            ImportedNamespaces.Add(qualifiedNs);

            ExpandNamespaceImports(context);
        }
    }

    private void ExpandNamespaceImports(CompilerContext context)
    {
        for (var i = 0; i < ImportedNamespaces.Count; i++)
        {
            var import = ImportedNamespaces[i];
            var imp = import.ToString();

            if (!imp.EndsWith(".*"))
            {
                continue;
            }

            var withoutWildcard = imp[..^2];

            context.Binder.TryResolveNamespace(ConversionUtils.QualifyNamespace(withoutWildcard),
                out var foundNamespaces);

            if (foundNamespaces == null)
            {
                //ToDo: add error that namespace has no subnamespace(s)
                return;
            }

            ImportedNamespaces.Remove(import);
            foreach (var foundNs in foundNamespaces.Namespaces)
            {
                ImportedNamespaces.Add(foundNs.Key.Qualify(withoutWildcard));
            }
        }
    }
}