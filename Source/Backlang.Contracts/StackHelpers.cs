namespace Backlang.Contracts;

public static class StackHelpers
{
    public static int SizeOf(IType type, TypeEnvironment environment)
    {
        if (type == environment.Boolean || type == environment.UInt8 || type == environment.Int8)
        {
            return 1;
        }
        else if (type == environment.UInt16 || type == environment.Int16)
        {
            return 2;
        }
        else if (type == environment.UInt32 || type == environment.Int32 || type == environment.Float32)
        {
            return 4;
        }
        else if (type == environment.UInt64 || type == environment.Int64 || type == environment.Float64)
        {
            return 8;
        }

        var size = 0;
        foreach (var field in type.Fields)
        {
            size += SizeOf(field.FieldType, environment);
        }

        return size;
    }
}