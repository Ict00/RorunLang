using RorunLang.Execution;

namespace RorunLang.Parse;

public interface IExpression
{
    public dynamic Do(ref Context ctx);
}

public class OpExpression(IExpression? lhs = null, IExpression? rhs = null, string op = "println") : IExpression
{
    public IExpression? lhs = lhs, rhs = rhs;
    public DOperator @do = Operators.Logic(op);
    
    public dynamic Do(ref Context ctx)
    {
        if (rhs == null)
        {
            return @do.Invoke([lhs.Do(ref ctx)], ref ctx);
        }
        lhs ??= SingleExpression.Of(0);
        rhs ??= SingleExpression.Of(0);
        return @do.Invoke([lhs.Do(ref ctx), rhs.Do(ref ctx)], ref ctx);
    }
}

public class SingleExpression : IExpression
{
    private readonly dynamic _value;

    private SingleExpression(dynamic value)
    {
        _value = value;
    }

    public static SingleExpression Of(dynamic value)
    {
        int val;
        if (int.TryParse(value.ToString(), out val))
        {
            return new SingleExpression(val);
        }
        
        return new SingleExpression(value);
    }

    public dynamic Do(ref Context ctx)
    {
        return _value;
    }
}