using Loyc.Syntax;

namespace Backlang.Driver;

public static class LNodeDeconstructors
{
    public static void Deconstruct(this LNode node, out string name, out LNode n2)
    {
        name = node.Name.Name;
        n2 = node.Args.Count >= 1 ? node.Args[0] : LNode.Missing;
    }

    public static void Deconstruct(this LNode node, out string name, out LNode n2, out LNode n3)
    {
        name = node.Name.Name;
        n2 = node.Args[0];
        n3 = node.Args[1];
    }

    public static void Deconstruct(this LNode node, out string name, out LNode n2, out LNode n3, out LNode n4)
    {
        name = node.Name.Name;
        n2 = node.Args[0];
        n3 = node.Args[1];
        n4 = node.Args[2];
    }
}