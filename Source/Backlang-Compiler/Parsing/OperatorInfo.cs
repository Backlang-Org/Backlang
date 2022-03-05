using Backlang_Compiler.Parsing;
namespace Backlang_Compiler.Parsing;

public record struct OperatorInfo(TokenType Token, int Precedence, bool IsUnary, bool IsPostUnary);
