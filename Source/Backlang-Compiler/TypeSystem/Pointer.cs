namespace Backlang_Compiler.TypeSystem;

public class Pointer
{
    public Pointer(Primitive type)
    {
        Type = type;
    }

    public Primitive Type { get; set; }
}