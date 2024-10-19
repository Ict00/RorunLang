namespace RorunLang.Parse;

public class RawBlock
{
    public string Name { get; set; } = "";
    public List<string> Modifiers { get; set; } = new();
    public List<Token> Body { get; set; } = new();
}