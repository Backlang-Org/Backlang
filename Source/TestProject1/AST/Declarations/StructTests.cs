using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.AST.Declarations;

[TestClass]
public class StructTests : ParserTestBase
{
    [TestMethod]
    public void Simple_Struct_Should_Pass()
    {
        var src = "struct Point { let X : i32; let Y : i32; }";
        var declaration = ParseAndGetNodes(src);
    }

    [TestMethod]
    public void Struct_With_Values_Should_Pass()
    {
        var src = "struct Point { let X : i32 = 24; let Y : i32 = 42; }";
        var declaration = ParseAndGetNodes(src);
    }

    [TestMethod]
    public void Struct_With_Mutable_Values_Should_Pass()
    {
        var src = "struct Point { let mut X : i32 = 24; let mut Y : i32 = 42; }";
        var declaration = ParseAndGetNodes(src);
    }

    [TestMethod]
    public void Struct_With_Modifiers_Should_Pass()
    {
        var src = "protected abstract struct Component { }";
        var declaration = ParseAndGetNodes(src);
    }
}