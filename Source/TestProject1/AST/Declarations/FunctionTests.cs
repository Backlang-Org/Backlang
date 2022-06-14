using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Declarations;

[TestClass]
public class FunctionTests : ParserTestBase
{
    [TestMethod]
    public void FunctionDeclaration_With_Multiple_Parameters_And_Default_Value_Should_Pass()
    {
        var src = "func test(something : i32, hello : bool = true) -> i32 { 123; }";
        var statement = ParseAndGetNodes(src);
    }

    [TestMethod]
    public void FunctionDeclaration_With_Multiple_Parameters_And_Trailing_Comma_Should_Pass()
    {
        var src = "func test(something : i32, hello : bool,) -> i32 { 123; }";
        var statement = ParseAndGetNodes(src);
    }

    [TestMethod]
    public void FunctionDeclaration_With_Multiple_Parameters_Should_Pass()
    {
        var src = "func test(something : i32, hello : bool) -> i32 { 123; }";
        var statement = ParseAndGetNodes(src);
    }

    [TestMethod]
    public void FunctionDeclaration_With_Parameter_And_Default_Value_Should_Pass()
    {
        var src = "func test(something : i32 = 42) -> i32 { 123; }";
        var statement = ParseAndGetNodes(src);
    }

    [TestMethod]
    public void FunctionDeclaration_With_Parameter_Should_Pass()
    {
        var src = "func test(something : i32) -> i32 { 123; }";
        var statement = ParseAndGetNodes(src);
    }

    [TestMethod]
    public void FunctionDeclaration_Without_Parameters_Should_Pass()
    {
        var src = "func test() -> i32 { 123; }";
        var statement = ParseAndGetNodes(src);
    }

    [TestMethod]
    public void FunctionDeclaration_With_Modifiers_Should_Pass()
    {
        var src = "public static func test() -> i32 { 123; }";
        var statement = ParseAndGetNodes(src);
    }
}