﻿using Backlang.Contracts.TypeSystem;

namespace Backlang.Driver.Core;

public static class ImplicitTypeCastTable
{
    private static readonly Dictionary<IType, IType[]> castMap = new();

    public static void InitCastMap(TypeEnvironment environment)
    {
        castMap.Add(environment.UInt8, new[] { environment.UInt16, environment.UInt32, environment.UInt64 });
        castMap.Add(environment.UInt16, new[] { environment.UInt32, environment.UInt64 });
        castMap.Add(environment.UInt32, new[] { environment.UInt64 });

        castMap.Add(environment.Int8,
            new[] { environment.Int16, environment.Int32, environment.Int64, environment.Float32 });
        castMap.Add(environment.Int16,
            new[] { environment.Int32, environment.Int64, environment.Float32, environment.Float64 });
        castMap.Add(environment.Int32, new[] { environment.Int64, environment.Float64 });
    }

    public static bool IsAssignableTo(this IType type, IType toCast)
    {
        if (type == toCast)
        {
            return true;
        }

        if (toCast is UnitType ut)
        {
            return IsAssignableTo(type, ut.BaseTypes[0]);
        }

        if (HasImplicitCastOperator(type, toCast))
        {
            return true;
        }

        if (castMap.ContainsKey(toCast))
        {
            return castMap[toCast].Contains(type);
        }

        if (toCast.FullName.ToString() == "System.Object")
        {
            return true;
        }

        return false;
    }

    private static bool HasImplicitCastOperator(IType type, IType toCast)
    {
        var result = type.TryGetOperator("implicit", out var method, type);

        return result && method.ReturnParameter.Type == toCast;
    }
}