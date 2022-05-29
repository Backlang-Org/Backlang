namespace Backlang.Codeanalysis.Parsing.Precedences;
public enum UnaryOpPrecedences
{

    Negative = 6, // !bool
    Minus = Negative, // -int

    Literals = 3, // u ui ub us ul b s l

    Ampersand = 9,
    Hat = Ampersand,

    Dollar = 10,

}
