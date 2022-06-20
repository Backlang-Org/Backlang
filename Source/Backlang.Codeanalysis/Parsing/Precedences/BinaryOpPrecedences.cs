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
    SwapOperator = 2,
    
    FunctionCalls = 7, // . ::

    PipeOperator = 8, // |>
    ShiftOperator = PipeOperator,

    OperationShortcuts = 9, // += -= *= /=
    Equals = OperationShortcuts,

}
