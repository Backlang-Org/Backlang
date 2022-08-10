namespace Backlang.Codeanalysis.Parsing.Precedences;

public enum BinaryOpPrecedences
{
    Hat = 1,
    SwapOperator = Hat,

    Ampersand = 2,

    EqualsEquals = 3,
    DashedOps = EqualsEquals, // add, sub
    Range = DashedOps,
    And = Range, // &&

    DottedOps = 4, // mul, div
    Percent = DottedOps,
    Or = Percent,
    Comparisons = Or, // < <= >= >

    OperationShortcuts = 5, // += -= *= /=

    PipeOperator = 6, // |>
    Equals = PipeOperator,

    FunctionCalls = 7,//  ::
    Dot = FunctionCalls,

    Casting = 8, // as
}