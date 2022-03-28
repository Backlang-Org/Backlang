using Backlang.Codeanalysis.Parsing.AST.Statements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Statements;

[TestClass]
public class ForStatementTests : ParserTestBase
{
    [TestMethod]
    public void For_With_If_Should_Pass()
    {
        var src = "for x in 1..5 { if !d { 42; } else { 1; } }";
        var tree = ParseAndGetNodeInFunction<ForStatement>(src);
    }

    [TestMethod]
    public void For_With_Type_In_Array_With_If_Should_Pass()
    {
        var src = "for x : i32 in arr { if !d { 42; } else { 1; } }";
        var tree = ParseAndGetNodeInFunction<ForStatement>(src);
    }

    [TestMethod]
    public void For_With_Type_With_If_Should_Pass()
    {
        var src = "for x : i32 in 1..5 { if !d { 42; } else { 1; } }";
        var tree = ParseAndGetNodeInFunction<ForStatement>(src);
    }
}