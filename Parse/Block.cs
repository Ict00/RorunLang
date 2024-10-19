using RorunLang.Execution;

namespace RorunLang.Parse;

public class Block
{
    public string Name { get; set; } = "main";
    public List<OpExpression> Expressions { get; set; } = new();
    public List<string> Modifiers { get; set; } = ["late", "open"];
    public Scope NextScope { get; set; } = new();

    public dynamic Execute()
    {
        var currentScope = new Scope();
        var nextScope = NextScope;
        var ctx = new Context(ref currentScope, ref nextScope);
        
        var r = Execute(ref ctx);
        NextScope = nextScope;
        
        return r;
    }
    
    public dynamic Execute(ref Context ctx)
    {
        if (Modifiers.Contains("early") && !Modifiers.Contains("crate"))
        {
            ctx.CurrentScope.Clear();
            ctx.NextScope.Clear();
        }

        dynamic r = "^";
        
        foreach (var it in Expressions)
        {
            r = it.Do(ref ctx);
            try
            {
                if (r != null && r != "^")
                {
                    break;
                }
            }
            catch
            {
                if (r.GetType() != typeof(List<dynamic>))
                {
                    break;
                }
            }
        }
        
        if (Modifiers.Contains("late") && !Modifiers.Contains("crate"))
        {
            ctx.CurrentScope.Clear();
            ctx.NextScope.Clear();
        }
        
        return r;
    }
}