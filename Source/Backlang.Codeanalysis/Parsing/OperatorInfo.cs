using Backlang.Codeanalysis.Parsing;
namespace Backlang.Codeanalysis.Parsing;

public record struct OperatorInfo(TokenType TokenType, int Precedence, bool IsUnary, bool IsPostUnary);
