namespace Backlang.Codeanalysis.Parsing.Precedences;
public enum UnaryOpPrecedences
{

    Negative = 6, // !bool
    Minus = Negative, // -int

    Ampersand = 9,
    Hat = Ampersand,

    Dollar = 10,

}
