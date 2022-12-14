namespace Backlang.Contracts.ConstraintSystem;

public class ConstraintCompiler
{
    public Dictionary<string, object> Variables = new();
    public Dictionary<string, LNode> Constraints = new();

    public object Evaluate(LNodeList constraints, object obj)
    {
        object result = false;
        foreach (var constraint in constraints)
        {
            result = Evaluate(constraint, obj);
        }

        return result;
    }

    private object Evaluate(LNode constraint, object obj)
    {
        if (constraint.ArgCount == 1 && constraint[0].HasValue)
        {
            return constraint[0].Value;
        }
        else if (constraint.ArgCount == 2 && constraint.Name.Name.StartsWith("'"))
        {
            return EvaluateBinary(constraint, obj);
        }
        else if (constraint.IsId)
        {
            return EvaluateVariable(constraint, obj);
        }
        else if (constraint.Name.Name == "#constraint")
        {
            var idNode = constraint[0];

            if (Constraints.TryGetValue(idNode.Name.Name, out var constraintDefinition))
            {
                return Evaluate(constraintDefinition, obj);
            }
        }

        return null;
    }

    private object EvaluateVariable(LNode constraint, object obj)
    {
        if (constraint.IsIdNamed("value"))
        {
            return obj;
        }

        if (Variables.TryGetValue(constraint.Name.Name, out var value))
        {
            return value;
        }

        return false;
    }

    private object EvaluateBinary(LNode constraint, object obj)
    {
        var lhsValue = (dynamic)Evaluate(constraint[0], obj);
        var rhsValue = (dynamic)Evaluate(constraint[1], obj);

        switch (constraint.Name.Name)
        {
            case "'==":
                return lhsValue == rhsValue;

            case "'!=":
                return lhsValue != rhsValue;

            case "'<":
                return lhsValue < rhsValue;

            case "'>":
                return lhsValue > rhsValue;

            case "'<=":
                return lhsValue <= rhsValue;

            case "'>=":
                return lhsValue >= rhsValue;

            case "'is":
                return EvaluateTypeConstraint(lhsValue, constraint[1]);

            default:
                return false;
        }
    }

    private object EvaluateTypeConstraint(object lhsValue, LNode rhsValue)
    {
        return lhsValue.GetType().IsAssignableTo(ConvertNodeToType(rhsValue));
    }

    private Type ConvertNodeToType(LNode constraintType)
    {
        switch (constraintType.Name.Name)
        {
            case "#bool": return typeof(bool);
            case "#int8": return typeof(byte);
            case "#int16": return typeof(short);
            case "#int32": return typeof(int);
            case "#int64": return typeof(long);
            case "#float16": return typeof(Half);
            case "#float32": return typeof(float);
            case "#float64": return typeof(double);
            default: return typeof(bool);
        }
    }
}