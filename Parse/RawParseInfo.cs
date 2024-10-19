namespace RorunLang.Parse;

public class RawParseInfo
{
    public readonly List<RawBlock> Blocks = new();
    public readonly List<RawStruct> Structs = new();
}

public class ParseInfo
{
    public readonly List<Block> Blocks = new();
    public readonly Dictionary<string, Struct> Structs = new();
}