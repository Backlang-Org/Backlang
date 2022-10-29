namespace Backlang.Driver;

public static class LNodeDeconstructors
{
    public static void Deconstruct(this LNode node, out string name, out LNode a0)
    {
        name = node.Name.Name;
        a0 = node.Args.Count >= 1 ? node.Args[0] : LNode.Missing;
    }

    public static void Deconstruct(this LNode node, out string name, out LNode a0, out LNode a1)
    {
        name = node.Name.Name;

        a0 = node.Args.Count >= 1 ? node.Args[0] : LNode.Missing;
        a1 = node.Args.Count >= 2 ? node.Args[1] : LNode.Missing;
    }

    public static void Deconstruct(this LNode node, out string name, out LNode a0, out LNode a1, out LNode a2)
    {
        name = node.Name.Name;

        a0 = node.Args.Count >= 1 ? node.Args[0] : LNode.Missing;
        a1 = node.Args.Count >= 2 ? node.Args[1] : LNode.Missing;
        a2 = node.Args.Count >= 3 ? node.Args[2] : LNode.Missing;
    }
}