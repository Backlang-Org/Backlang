namespace Backlang.Driver;
public static class QualifiedNameDeconstructors
{
    public static void Deconstruct(this QualifiedName qualified, out string qualifier, out string unqualified)
    {
        qualifier = qualified.Qualifier.ToString();
        unqualified = qualified.FullyUnqualifiedName.ToString();
    }
}
