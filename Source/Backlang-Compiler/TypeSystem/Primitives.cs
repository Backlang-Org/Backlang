namespace Backlang_Compiler.TypeSystem;

public static class Primitives
{
    public static Primitive Bool = new Primitive("bool", PrimitiveObjectID.Bool);
    public static Primitive I32 = new Primitive("i32", PrimitiveObjectID.I32);
    public static Primitive I64 = new Primitive("i64", PrimitiveObjectID.I64);
    public static Primitive None = new Primitive("none", PrimitiveObjectID.None);
    public static Primitive String = new Primitive("string", PrimitiveObjectID.String);
}