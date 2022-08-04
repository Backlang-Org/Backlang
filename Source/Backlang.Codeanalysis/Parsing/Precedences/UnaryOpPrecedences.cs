namespace Backlang.Codeanalysis.Parsing.Precedences;
public enum UnaryOpPrecedences
{
    Literals = 3, // u ui ub us ul b s l

    LogicalNot = 6, // !bool
    Negate = LogicalNot, // -int

    Ampersand = 9,
    Hat = Ampersand,

    Dollar = 10,
}
