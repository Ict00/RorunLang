using RorunLang.Parse;

namespace RorunLang.Execution;

public static class Executor
{
    public static List<Block> Blocks { get; set; } = new();
    public static Dictionary<string, Struct> Structs { get; set; } = new();

    public static void Launch()
    {
        Blocks.Find(x => x.Name == "main").Execute();
    }
}