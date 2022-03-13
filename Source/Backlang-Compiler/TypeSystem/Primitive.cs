namespace Backlang_Compiler.TypeSystem;

public class Primitive
{
    public Primitive(string name, PrimitiveObjectID objectID)
    {
        Name = name;
        ObjectID = objectID;
    }

    public string Name { get; set; }
    public PrimitiveObjectID ObjectID { get; set; }
}