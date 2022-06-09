namespace Backlang.Codeanalysis.Parsing.Precedences;
public enum BinaryOpPrecedences
{

    Hat = 2,

    Ampersand = 3,

    EqualsEquals = 4,
    DashedOps = EqualsEquals, // add, sub
    Range = DashedOps,
    And = Range, // &&

    DottedOps = 5, // mul, div
    Percent = DottedOps,
    Or = Percent,
    Comparisons = Or, // < <= >= >
    
    FunctionCalls = 7, // . ::

    PipeOperator = 8, // |>

    OperationShortcuts = 9, // += -= *= /=
    Equals = OperationShortcuts,

}
