using LeMP;
using Loyc;
using Loyc.Syntax;
using System.Reflection;
using S = Loyc.Syntax.CodeSymbols;

namespace Backlang.Core.Macros;

public partial class BuiltInMacros
{
    private static readonly LNode _CodeSymbols = F.Id("CodeSymbols");

    private static Dictionary<Symbol, Symbol> CodeSymbolTable = null;

    [LexicalMacro("quote(code); quote { code(); }",
                    "Macro-based code quote mechanism, to be used as long as a more complete compiler is not availabe. " +
        "If there is a single parameter that is braces, the braces are stripped out. " +
        "If there are multiple parameters, or multiple statements in braces, the result is a call to #splice(). " +
        "The output refers unqualified to `CodeSymbols` and `LNode` so you must have 'using Loyc.Syntax' at the top of your file. " +
        "The substitution operator $(expr) causes the specified expression to be inserted unchanged into the output.",
        "quote", "#quote")]
    public static LNode Quote(LNode node, IMessageSink sink) => Quote(node, true, true);

    /// <summary>This implements `quote` and related macros. It converts a
    /// Loyc tree (e.g. `Foo(this)`) into a piece of .NET code that can
    /// construct the essence of the same tree (e.g.
    /// `LNode.Call((Symbol) "Foo", LNode.List(LNode.Id(CodeSymbols.This)));`),
    /// although this code does not preserve the Range properties.
    /// <param name="allowSubstitutions">If this flag is true, when a calls to
    ///   <c>@`'$`</c> is encountered, it is treated as a substitution request,
    ///   e.g. <c>$node</c> causes <c>node</c> to be included unchanged in the
    ///   output, and by example, <c>Foo($x, $(..list))</c> produces this tree:
    ///   <c>LNode.Call((Symbol) "Foo", LNode.List().Add(x).AddRange(list))</c>.
    ///   </param>
    /// <param name="ignoreTrivia">If this flag is true, the output expression
    ///   does not construct trivia attributes.</param>
    /// <returns>The quoted form.</returns>
    public static LNode Quote(LNode node, bool allowSubstitutions, bool ignoreTrivia)
    {
        LNode code = node, arg;
        if (code.ArgCount == 1 && (arg = code.Args[0]).Calls(S.Braces) && !arg.HasPAttrs())
        {
            // Braces are needed to allow statement syntax in EC#; they are
            // not necessarily desired in the output, so ignore them. The user
            // can still write quote {{...}} to include braces in the output.
            code = arg;
        }
        return new CodeQuoter(allowSubstitutions, ignoreTrivia).Quote(code.Args.AsLNode(S.Splice));
    }

    [LexicalMacro(@"e.g. quoteWithTrivia(/* cool! */ $foo) ==> foo.PlusAttrs(LNode.List(LNode.Trivia(CodeSymbols.TriviaMLComment, "" cool! "")))",
        "Behaves the same as quote(code) except that trivia is included in the output.")]
    public static LNode QuoteWithTrivia(LNode node, IMessageSink sink) => Quote(node, true, false);

    [LexicalMacro(@"e.g. rawQuote($foo) ==> F.Call(CodeSymbols.Substitute, F.Id(""foo""));",
                "Behaves the same as quote(code) except that the substitution operator $ is not recognized as a request for substitution.",
        "rawQuote", "#rawQuote")]
    public static LNode RawQuote(LNode node, IMessageSink sink) => Quote(node, false, true);

    [LexicalMacro(@"e.g. rawQuoteWithTrivia(/* cool! */ $foo) ==> LNode.Call(LNode.List(LNode.Trivia(CodeSymbols.TriviaMLComment, "" cool! "")), CodeSymbols.Substitute, LNode.List(LNode.Id((Symbol) ""foo"")))",
        "Behaves the same as rawQuote(code) except that trivia is included in the output.")]
    public static LNode RawQuoteWithTrivia(LNode node, IMessageSink sink) => Quote(node, false, false);

    internal static LNode QuoteSymbol(Symbol name)
    {
        if (CodeSymbolTable == null)
            CodeSymbolTable = FindStaticReadOnlies<Symbol>(typeof(CodeSymbols), fInfo => !fInfo.Name.StartsWith("_"));
        if (CodeSymbolTable.TryGetValue(name, out var field))
            return F.Dot(_CodeSymbols, F.Id(field));
        else
            return F.Call(S.Cast, F.Literal(name.Name), F.Id("Symbol"));
    }

    /// <summary>Helper function that finds the static readonly fields of a given
    /// type in a given class, and creates a table from the _values_ of those
    /// fields to the _names_ of those fields.</summary>
    private static Dictionary<T, Symbol> FindStaticReadOnlies<T>(Type type, Predicate<FieldInfo> filter = null)
    {
        var dict = new Dictionary<T, Symbol>();
        var list = type.GetFields(BindingFlags.Static | BindingFlags.Public)
            .Where(field => typeof(T).IsAssignableFrom(field.FieldType) && field.IsInitOnly
                && field.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length == 0);
        foreach (var field in list)
            if (filter == null || filter(field))
                dict[(T)field.GetValue(null)] = GSymbol.Get(field.Name);
        return dict;
    }

    public class CodeQuoter
    {
        public bool _doSubstitutions;
        public bool _ignoreTrivia;
        private static readonly LNode CodeSymbols_Splice = F.Dot(_CodeSymbols, F.Id("Splice"));

        private static readonly LNode Id_LNode = F.Id("LNode");

        private static readonly LNode Id_PlusAttrs = F.Id("PlusAttrs");

        private static readonly LNode LNode_Braces = F.Dot(Id_LNode, F.Id("Braces"));

        private static readonly LNode LNode_Call = F.Dot(Id_LNode, F.Id("Call"));

        private static readonly LNode LNode_Dot = F.Dot(Id_LNode, F.Id("Dot"));

        private static readonly LNode LNode_Id = F.Dot(Id_LNode, F.Id("Id"));

        private static readonly LNode LNode_InParensTrivia = F.Dot(Id_LNode, F.Id("InParensTrivia"));

        private static readonly LNode LNode_List = F.Dot(Id_LNode, F.Id("List"));

        private static readonly LNode LNode_Literal = F.Dot(Id_LNode, F.Id("Literal"));

        private static readonly LNode LNode_Missing = F.Dot(Id_LNode, F.Id("Missing"));

        private static readonly LNode LNode_Trivia = F.Dot(Id_LNode, F.Id("Trivia"));

        public CodeQuoter(
                                                                                                                            bool doSubstitutions,
            bool ignoreTrivia = true)
        {
            _doSubstitutions = doSubstitutions;
            _ignoreTrivia = ignoreTrivia;
        }

        public LNode Quote(LNode node)
        {
            // TODO: When quoting, ignore injected trivia (trivia with the TriviaInjected flag)
            if (node.Equals(LNode.InParensTrivia))
                return LNode_InParensTrivia;
            if (node.Equals(LNode.Missing))
                return LNode_Missing;

            LNodeList creationArgs = new LNodeList();

            // Translate attributes (if any)
            var attrList = MaybeQuoteList(node.Attrs, isAttributes: true);
            if (attrList != null)
                creationArgs.Add(attrList);

            LNode result;
            switch (node.Kind)
            {
                case LNodeKind.Literal: // => F.Literal(value)
                    creationArgs.Add(node.WithoutAttrs());
                    result = F.Call(LNode_Literal, creationArgs);
                    break;

                case LNodeKind.Id: // => F.Id(string), F.Id(CodeSymbols.Name)
                    creationArgs.Add(QuoteSymbol(node.Name));
                    result = F.Call(LNode_Id, creationArgs);
                    break;

                default: // NodeKind.Call => F.Dot(...), F.Of(...), F.Call(...), F.Braces(...)
                    bool preserveStyle = true;
                    if (_doSubstitutions && node.Calls(S.Substitute, 1))
                    {
                        preserveStyle = false;
                        result = node.Args[0];
                        if (attrList != null)
                        {
                            if (result.IsCall)
                                result = result.InParens();
                            result = F.Call(F.Dot(result, Id_PlusAttrs), attrList);
                        }
                    }
                    else if (!_ignoreTrivia && node.ArgCount == 1 && node.TriviaValue != NoValue.Value && node.Target.IsId)
                    {
                        // LNode.Trivia(Symbol, object)
                        result = F.Call(LNode_Trivia, QuoteSymbol(node.Name), F.Literal(node.TriviaValue));
                    }
                    /*else if (node.Calls(S.Braces)) // F.Braces(...)
						result = F.Call(LNode_Braces, node.Args.SmartSelect(arg => QuoteOne(arg, substitutions)));
					else if (node.Calls(S.Dot) && node.ArgCount.IsInRange(1, 2))
						result = F.Call(LNode_Dot, node.Args.SmartSelect(arg => QuoteOne(arg, substitutions)));
					else if (node.Calls(S.Of))
						result = F.Call(LNode_Of, node.Args.SmartSelect(arg => QuoteOne(arg, substitutions)));*/
                    else
                    {
                        // General case: F.Call(<Target>, <Args>)
                        if (node.Target.IsId)
                            creationArgs.Add(QuoteSymbol(node.Name));
                        else
                            creationArgs.Add(Quote(node.Target));

                        var argList = MaybeQuoteList(node.Args);
                        if (argList != null)
                            creationArgs.Add(argList);
                        result = F.Call(LNode_Call, creationArgs);
                    }
                    // Note: don't preserve prefix notation because if $op is +,
                    // we want $op(x, y) to generate code for x + y (there is no
                    // way to express this with infix notation.)
                    if (preserveStyle && node.BaseStyle != NodeStyle.Default && node.BaseStyle != NodeStyle.PrefixNotation)
                        result = F.Call(F.Dot(result, F.Id("SetStyle")), F.Dot(F.Id("NodeStyle"), F.Id(node.BaseStyle.ToString())));
                    break;
            }
            return result;
        }

        private static LNode VarArgExpr(LNode arg)
        {
            LNode subj;
            if (arg.Calls(S.Substitute, 1) && ((subj = arg.Args[0]).Calls(S.DotDot, 1) || subj.Calls(S.DotDotDot, 1)))
                return subj.Args[0];
            return null;
        }

        private LNode MaybeQuoteList(LNodeList list, bool isAttributes = false)
        {
            if (isAttributes && _ignoreTrivia)
                list = list.SmartWhere(n => !n.IsTrivia || n.IsIdNamed(S.TriviaInParens));
            if (list.IsEmpty)
                return null;
            else if (_doSubstitutions && list.Any(a => VarArgExpr(a) != null))
            {
                if (list.Count == 1)
                    return F.Call(LNode_List, VarArgExpr(list[0]));
                // If you write something like quote(Foo($x, $(...y), $z)), a special
                // output style is used to accommodate the variable argument list.
                LNode argList = F.Call(LNode_List);
                foreach (LNode arg in list)
                {
                    var vae = VarArgExpr(arg);
                    if (vae != null)
                        argList = F.Call(F.Dot(argList, F.Id("AddRange")), vae);
                    else
                        argList = F.Call(F.Dot(argList, F.Id("Add")), Quote(arg));
                }
                return argList;
            }
            else
                return F.Call(LNode_List, list.SmartSelect(item => Quote(item)));
        }
    }
}