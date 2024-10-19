using RorunLang.Parse;

namespace RorunLang.Execution;

public class Scope
{
    public Dictionary<string, Variable> variables = new()
    {
        { "true", new Variable(true, true) },
        { "false", new Variable(false, true) },
    };

    public void Clear()
    {
        Dictionary<string, Variable> newVariables = new();
        foreach (var variable in variables)
        {
            if (variable.Value.IsStatic)
            {
                newVariables.Add(variable.Key, variable.Value);
            }
        }
        variables = new Dictionary<string, Variable>(newVariables);
    }
    
    public void Del(dynamic name)
    {
        variables.Remove(name);
    }

    public void Put(dynamic name, dynamic value)
    {
        variables[name.ToString()] = new Variable(value);
    }
    
    public void Put<T>(dynamic name, dynamic value) where T : class
    {
        variables[name.ToString()] = new Variable(value as T ?? throw new NullReferenceException("WHAT THE HEEEEEEELLL????"));
    }

    public dynamic Get(dynamic name)
    {
        try
        {
            if (variables[name.ToString()].Value.GetType() == typeof(Struct))
            {
                return variables[name.ToString()].Value as Struct;
            }
            
            if (variables[name.ToString()].Value.GetType() == typeof(int))
            {
                return Convert.ToInt32(variables[name.ToString()].Value);
            }
            
            return variables[name.ToString()].Value;
        }
        catch
        {
            return name;
        }
    }
    
    public dynamic Get<T>(dynamic name) where T : class
    {
        try
        {
            return variables[name.ToString()].Value as T ?? throw new NullReferenceException("WHAT THE HEEEEEEELLL????");
        }
        catch
        {
            return name;
        }
    }
}

public class Variable(dynamic value, bool isStatic = false)
{
    public dynamic Value { get; set; } = value;
    public bool IsStatic => isStatic;
}