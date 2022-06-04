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

    FractionSymbol = 6, // Symbol: \\ 
    
    FunctionCalls = 7, // . ::

    OperationShortcuts = 8, // += -= *= /=
    Equals = OperationShortcuts,

}
