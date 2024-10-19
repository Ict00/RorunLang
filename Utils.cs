using RorunLang.Execution;

namespace RorunLang;

public static class Utils
{
    public static readonly List<string> Modifiers = 
    [
        "block",
        "open",
        "closed",
        "crate",
        "late",
        "early",
        "struct"
    ];

    public static readonly List<char> Operators =
    [
        '+', '=', '-', '*', '/', ',', '.', ':', '<', '>', '&', '|', '[', ']', '`'
    ];

    public static readonly List<string> MOperators =
    [
        "print",
        "out",
        "outln",
        "println",
        ":", "",
        "->", "<-",
        "=>", "+=",
        "-=", "*=",
        "/=", "..",
        "]=", "]+",
        "]-", "]|",
        "||", "&&",
        ">=", "==",
        "<=", "!=",
        "!", ".",
        "jsonit", "get",
        "args", "as",
        "`", "return",
        "at", "rev",
        "destr", "do",
        "len", "in",
        "run", "runif",
        "instance", "del",
        "destruct", "import",
        "format", "unload", 
        "cast", "map", "has"
    ];
}

public enum TokenType
{
    Undefined,
    EndLine,
    Identifier,
    String,
    Operator,
    Modifier,
    LB,
    RB,
}

public struct Token
{
    public TokenType Type;
    public string Value;
}

public ref struct Context(ref Scope scope, ref Scope nextScope)
{
    public ref Scope CurrentScope = ref scope;
    public ref Scope NextScope = ref nextScope;
}