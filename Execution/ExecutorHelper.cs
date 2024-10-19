using RorunLang.Parse;

namespace RorunLang.Execution;

public class ExecutorHelper
{
    public static void Reset()
    {
        Executor.Blocks.Clear();
        Executor.Structs.Clear();
    }
    
    public static void Import(string fileName)
    {
        if (File.Exists(fileName))
        {
            var code = File.ReadAllText(fileName);
            LowImport(Parser.SecondStage(Parser.FirstStage(Lexer.FixTokens(Lexer.GetTokens(code)))));
        }
        else
        {
            if (File.Exists($"{fileName}.kii"))
            {
                var code = File.ReadAllText($"{fileName}.kii");
                LowImport(Parser.SecondStage(Parser.FirstStage(Lexer.FixTokens(Lexer.GetTokens(code)))));
            }
            else
            {
                if (File.Exists($"{fileName}.rr"))
                {
                    var code = File.ReadAllText($"{fileName}.rr");
                    LowImport(Parser.SecondStage(Parser.FirstStage(Lexer.FixTokens(Lexer.GetTokens(code)))));
                }
                else
                {
                    throw new FileNotFoundException($"File {fileName} not found; Can't import!");
                }
            }
        }
    }
    
    public static void Build(string code)
    {
        LowImport(Parser.SecondStage(Parser.FirstStage(Lexer.FixTokens(Lexer.GetTokens(code)))));
    }

    private static void LowImport(ParseInfo parseInfo)
    {
        foreach (var structure in parseInfo.Structs)
        {
            Executor.Structs.Add(structure.Key, structure.Value);
        }
        Executor.Blocks.AddRange(parseInfo.Blocks);
    }

    public static void Run()
    {
        Executor.Launch();
    }
}