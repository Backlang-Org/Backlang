using Loyc.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace TestProject1.AST.Declarations;

[TestClass]
public class FunctionTests : ParserTestBase
{
    [TestMethod]
    public void FunctionDeclaration_With_Multiple_Parameters_And_Default_Value_Should_Pass()
    {
        var src = "func test(something : i32, hello : bool = true) -> i32 { 123; }";
        var statement = ParseAndGetNode(src);

        var retType = statement.Args[0];
        var name = statement.Args[1];
        var prms = statement.Args[2];

        Assert.AreEqual("i32", retType.Args[0].Args[0].Name.Name);
        Assert.AreEqual("test", name.Args[0].Args[0].Name.Name);

        LNode prm;
        // First Parameter 'something : i32'
        prm = prms.Args[0];
        Assert.AreEqual("i32", prm.Args[0].Args[0].Args[0].Name.Name);
        Assert.AreEqual("something", prm.Args[1].Args[0].Name.Name);
        // Second Parameter 'hello : bool = true'
        prm = prms.Args[1];
        Assert.AreEqual("bool", prm.Args[0].Args[0].Args[0].Name.Name);
        Assert.AreEqual("hello", prm.Args[1].Args[0].Name.Name);
        Assert.AreEqual(true, prm.Args[1].Args[1].Args[0].Value);
    }

    [TestMethod]
    public void FunctionDeclaration_With_Multiple_Parameters_And_Trailing_Comma_Should_Pass()
    {
        var src = "func test(something : i32, hello : bool,) -> i32 { 123; }";
        var statement = ParseAndGetNode(src);

        var retType = statement.Args[0];
        var name = statement.Args[1];
        var prms = statement.Args[2];

        Assert.AreEqual("i32", retType.Args[0].Args[0].Name.Name);
        Assert.AreEqual("test", name.Args[0].Args[0].Name.Name);

        LNode prm;
        // First Parameter 'something : i32'
        prm = prms.Args[0];
        Assert.AreEqual("i32", prm.Args[0].Args[0].Args[0].Name.Name);
        Assert.AreEqual("something", prm.Args[1].Args[0].Name.Name);
        // Second Parameter 'hello : bool'
        prm = prms.Args[1];
        Assert.AreEqual("bool", prm.Args[0].Args[0].Args[0].Name.Name);
        Assert.AreEqual("hello", prm.Args[1].Args[0].Name.Name);
    }

    [TestMethod]
    public void FunctionDeclaration_With_Multiple_Parameters_Should_Pass()
    {
        var src = "func test(something : i32, hello : bool) -> i32 { 123; }";
        var statement = ParseAndGetNode(src);

        var retType = statement.Args[0];
        var name = statement.Args[1];
        var prms = statement.Args[2];

        Assert.AreEqual("i32", retType.Args[0].Args[0].Name.Name);
        Assert.AreEqual("test", name.Args[0].Args[0].Name.Name);

        LNode prm;
        // First Parameter 'something : i32'
        prm = prms.Args[0];
        Assert.AreEqual("i32", prm.Args[0].Args[0].Args[0].Name.Name);
        Assert.AreEqual("something", prm.Args[1].Args[0].Name.Name);
        // Second Parameter 'hello : bool'
        prm = prms.Args[1];
        Assert.AreEqual("bool", prm.Args[0].Args[0].Args[0].Name.Name);
        Assert.AreEqual("hello", prm.Args[1].Args[0].Name.Name);
    }

    [TestMethod]
    public void FunctionDeclaration_With_Parameter_And_Default_Value_Should_Pass()
    {
        var src = "func test(something : i32 = 42) -> i32 { 123; }";
        var statement = ParseAndGetNode(src);

        var retType = statement.Args[0];
        var name = statement.Args[1];
        var prms = statement.Args[2];

        Assert.AreEqual("i32", retType.Args[0].Args[0].Name.Name);
        Assert.AreEqual("test", name.Args[0].Args[0].Name.Name);

        // First Parameter 'something : i32 = 42'
        var prm = prms.Args[0];
        Assert.AreEqual("i32", prm.Args[0].Args[0].Args[0].Name.Name);
        Assert.AreEqual("something", prm.Args[1].Args[0].Name.Name);
        Assert.AreEqual(42, prm.Args[1].Args[1].Args[0].Value);
    }

    [TestMethod]
    public void FunctionDeclaration_With_Parameter_Should_Pass()
    {
        var src = "func test(something : i32) -> i32 { 123; }";
        var statement = ParseAndGetNode(src);

        var retType = statement.Args[0];
        var name = statement.Args[1];
        var prms = statement.Args[2];

        Assert.AreEqual("i32", retType.Args[0].Args[0].Name.Name);
        Assert.AreEqual("test", name.Args[0].Args[0].Name.Name);

        // First Parameter 'something : i32'
        var prm = prms.Args[0];
        Assert.AreEqual("i32", prm.Args[0].Args[0].Args[0].Name.Name);
        Assert.AreEqual("something", prm.Args[1].Args[0].Name.Name);
    }

    [TestMethod]
    public void FunctionDeclaration_Without_Parameters_Should_Pass()
    {
        var src = "func test() -> i32 { 123; }";
        var statement = ParseAndGetNode(src);

        var retType = statement.Args[0];
        var name = statement.Args[1];
        var prms = statement.Args[2];

        Assert.AreEqual("i32", retType.Args[0].Args[0].Name.Name);
        Assert.AreEqual("test", name.Args[0].Args[0].Name.Name);

        Assert.IsTrue(prms.Args.IsEmpty);
    }

    [TestMethod]
    public void FunctionDeclaration_With_Modifiers_Should_Pass()
    {
        var src = "public static func test() -> i32 { 123; }";
        var statement = ParseAndGetNode(src);

        var retType = statement.Args[0];
        var name = statement.Args[1];
        var prms = statement.Args[2];

        var array = statement.Attrs.ToVList().ToArray();
        Assert.IsTrue(array.Any(_ => _.IsId && (_.Name == CodeSymbols.Public)));
        Assert.IsTrue(array.Any(_ => _.IsId && (_.Name == CodeSymbols.Static)));
        
        Assert.AreEqual("i32", retType.Args[0].Args[0].Name.Name);
        Assert.AreEqual("test", name.Args[0].Args[0].Name.Name);

        Assert.IsTrue(prms.Args.IsEmpty);
    }
}