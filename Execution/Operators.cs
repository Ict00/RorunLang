using System.Text.Json;
using RorunLang.Parse;

namespace RorunLang.Execution;
// ReSharper disable all
public delegate dynamic DOperator(List<dynamic> args, ref Context ctx);

public struct Operator
{
    public DOperator op;
    public int priority;
}

public static class Operators
{
    private static readonly Dictionary<string, Operator> _operators = new();

    private static DOperator PRINTLN = (List<dynamic> args, ref Context ctx) =>
    {
        List<dynamic> arg = new();
        foreach (var a in args)
        {
            arg.Add(ctx.CurrentScope.Get(a).ToString());
        }
        Console.WriteLine(string.Join(" ", arg));
        return "^";
    };
    
    private static DOperator PRINT = (List<dynamic> args, ref Context ctx) =>
    {
        List<dynamic> arg = new();
        foreach (var a in args)
        {
            arg.Add(ctx.CurrentScope.Get(a).ToString());
        }
        Console.Write(string.Join(" ", arg));
        return "^";
    };

    private static DOperator JSONIT = (List<dynamic> args, ref Context ctx) =>
    {
        return JsonSerializer.Serialize(ctx.CurrentScope.Get(args[0]), new JsonSerializerOptions()
        {
            IncludeFields = true,
        });
    };
    
    private static DOperator MAP_OP = (List<dynamic> args, ref Context ctx) =>
    {
        List<dynamic> result = new();

        if (ctx.CurrentScope.Get(args[0]).GetType() == typeof(List<dynamic>))
        {
            var arg = ctx.CurrentScope.Get(args[0]) as List<dynamic>;
            var mapper = args[1].ToString();
            var tryFind = Executor.Blocks.Find(x => x.Name == mapper.ToString());
            if (tryFind != null)
            {
                foreach (var a in arg)
                {
                    result.Add(Operators.Logic(":").Invoke([mapper, a], ref ctx));
                }
                return result;
            }
            else
            {
                throw new Exception($"Can't find {mapper}");
            }
        }
        throw new Exception($"{args[0]} is not a list");
        
    };

    private static DOperator GROUP = (List<dynamic> args, ref Context ctx) =>
    {
        var a = ctx.CurrentScope.Get(args[0]);
        var b = ctx.CurrentScope.Get(args[1]);
        var res = new List<dynamic>();

        if (b.GetType() == typeof(List<dynamic>))
        {
            res = [a, ..b];
        }
        else
        {
            res = [a, b];
        }

        return res;
    };
    
    private static DOperator NAME_GROUP = (List<dynamic> args, ref Context ctx) =>
    {
        var a = args[0];
        var b = args[1];
        var res = new List<dynamic>();

        if (b.GetType() == typeof(List<dynamic>))
        {
            res = [a, ..b];
        }
        else
        {
            res = [a, b];
        }

        return res;
    };
    
    private static DOperator ARGS_OP = (List<dynamic> args, ref Context ctx) =>
    {
        List<dynamic> a;
        if (args[0].GetType() == typeof(List<dynamic>))
        {
            a = args[0] as List<dynamic>;
        }
        else
        {
            a = [args[0]];
        }

        for (int i = 0; i < a.Count; i++)
        {
            try
            {
                ctx.CurrentScope.Put(a[i], ctx.NextScope.variables.ElementAt(i+2).Value.Value);
            }
            catch
            {
                
            }
        }
        
        return "^";
    };
    
    private static DOperator RUN_WITH_ARGS = (List<dynamic> args, ref Context ctx) =>
    {
        var block = args[0];
        List<dynamic> arg;
        if (args[1].GetType() == typeof(List<dynamic>))
        {
            arg = args[1] as List<dynamic>;
        }
        else
        {
            arg = [args[1]];
        }

        var c = Executor.Blocks.Find(x => x.Name.ToString() == block.ToString());
        
        foreach (var a in arg)
        {
            c.NextScope.Put(a, ctx.CurrentScope.Get(a));
        }
        
        return c.Execute();
    };
    
    private static DOperator RETURN = (List<dynamic> args, ref Context ctx) =>
    {
        return ctx.CurrentScope.Get(args[0]);
    };

    private static DOperator MULTIPLY = (List<dynamic> args, ref Context ctx) =>
    {
        return ctx.CurrentScope.Get(args[0]) * ctx.CurrentScope.Get(args[1]);
    };
    
    private static DOperator GETVAR = (List<dynamic> args, ref Context ctx) =>
    {
        return ctx.CurrentScope.Get(ctx.CurrentScope.Get(args[0]));
    };

    private static DOperator ADDITION = (List<dynamic> args, ref Context ctx) =>
    {
        if (args[0].GetType() == typeof(string))
        {
            return ctx.CurrentScope.Get(args[0]).ToString() + ctx.CurrentScope.Get(args[1]).ToString();
        }
        return Convert.ToInt32(ctx.CurrentScope.Get(args[0]) + ctx.CurrentScope.Get(args[1]));
    };
    
    private static DOperator SUB = (List<dynamic> args, ref Context ctx) =>
    {
        return ctx.CurrentScope.Get(args[0]) - ctx.CurrentScope.Get(args[1]);
    };
    
    private static DOperator DIVISION = (List<dynamic> args, ref Context ctx) =>
    {
        return ctx.CurrentScope.Get(args[0]) / ctx.CurrentScope.Get(args[1]);
    };

    private static DOperator ASSIGN = (List<dynamic> args, ref Context ctx) =>
    {
        ctx.CurrentScope.Put(args[0], ctx.CurrentScope.Get(args[1]));
        return "^";
    };
    
    private static DOperator ADD_ASSIGN = (List<dynamic> args, ref Context ctx) =>
    {
        ctx.CurrentScope.Put(args[0], ctx.CurrentScope.Get(args[0])+ctx.CurrentScope.Get(args[1]));
        return "^";
    };
    
    private static DOperator SUB_ASSIGN = (List<dynamic> args, ref Context ctx) =>
    {
        ctx.CurrentScope.Put(args[0], ctx.CurrentScope.Get(args[0])-ctx.CurrentScope.Get(args[1]));
        return "^";
    };
    
    private static DOperator DIV_ASSIGN = (List<dynamic> args, ref Context ctx) =>
    {
        ctx.CurrentScope.Put(args[0], ctx.CurrentScope.Get(args[0])/ctx.CurrentScope.Get(args[1]));
        return "^";
    };
    
    private static DOperator MUL_ASSIGN = (List<dynamic> args, ref Context ctx) =>
    {
        ctx.CurrentScope.Put(args[0], ctx.CurrentScope.Get(args[0])*ctx.CurrentScope.Get(args[1]));
        return "^";
    };
    
    private static DOperator RUN_FORWARD = (List<dynamic> args, ref Context ctx) =>
    {
        if (args[0].GetType() == typeof(List<dynamic>))
        {
            var vars = args[0] as List<dynamic>;
            var block = args[1] as string;
            foreach (var i in vars)
            {
                Executor.Blocks.Find(x => x.Name.ToString() == block.ToString()).NextScope.Put(i, ctx.CurrentScope.Get(i));
            }
            return Executor.Blocks.Find(x => x.Name.ToString() == block.ToString()).Execute();
        }
        else if (args[1].GetType() == typeof(List<dynamic>))
        {
            var @var = ctx.CurrentScope.Get(args[0]);
            var blocks = args[1] as List<dynamic>;

            foreach (var i in blocks)
            {
                Executor.Blocks.Find(x => x.Name.ToString() == i.ToString()).NextScope.Put(args[0], @var);
            }
        }
        else
        {
            Executor.Blocks.Find(x => x.Name.ToString() == args[1].ToString()).NextScope.Put(args[0], ctx.CurrentScope.Get(args[0]));
            return Executor.Blocks.Find(x => x.Name.ToString() == args[1].ToString()).Execute();
        }
        return "^";
    };
    
    private static DOperator FORWARD = (List<dynamic> args, ref Context ctx) =>
    {
        if (args[0].GetType() == typeof(List<dynamic>))
        {
            var vars = args[0] as List<dynamic>;
            var block = args[1] as string;
            foreach (var i in vars)
            {
                Executor.Blocks.Find(x => x.Name.ToString() == block.ToString()).NextScope.Put(i, ctx.CurrentScope.Get(i));
            }
        }
        else if (args[1].GetType() == typeof(List<dynamic>))
        {
            var @var = ctx.CurrentScope.Get(args[0]);
            var blocks = args[1] as List<dynamic>;

            foreach (var i in blocks)
            {
                Executor.Blocks.Find(x => x.Name.ToString() == i.ToString()).NextScope.Put(args[0], @var);
            }
        }
        else
        {
            Executor.Blocks.Find(x => x.Name.ToString() == args[1].ToString()).NextScope.Put(args[0], ctx.CurrentScope.Get(args[0]));
        }
        return "^";
    };
    
    private static DOperator LEN_OP = (List<dynamic> args, ref Context ctx) =>
    {
        try
        {
            return (ctx.CurrentScope.Get(args[0]) as List<dynamic>).Count;
        }
        catch
        {
            return (ctx.CurrentScope.Get(args[0]) as string).Length;
        }
    };
    
    
    private static DOperator DE_STR = (List<dynamic> args, ref Context ctx) =>
    {
        return args[0].ToString().Replace("\"", "");
    };
    
    private static DOperator AT_OPERATOR = (List<dynamic> args, ref Context ctx) =>
    {
        return (ctx.CurrentScope.Get(args[1]) as List<dynamic>)[ctx.CurrentScope.Get(args[0])];
    };
    
    private static DOperator DO_OPERATOR = (List<dynamic> args, ref Context ctx) =>
    {
        string op = ctx.CurrentScope.Get(args[0]).ToString();
        List<dynamic> arg = ctx.CurrentScope.Get(args[1]);
        
        if (arg.Count > 2)
        {
            var a1 = arg[0];
            List<dynamic> a2 = arg.Slice(1, arg.Count - 1);
            arg.Clear();
            
            arg = [
                a1, a2
            ];
        }
        
        return Operators.Logic(op).Invoke(arg, ref ctx);
    };
    
    private static DOperator ADD_TO = (List<dynamic> args, ref Context ctx) =>
    {
        var a = ctx.CurrentScope.Get(args[0]) as List<dynamic>;
        var b = ctx.CurrentScope.Get(args[1]);
        
        a.Add(b);
        
        return a;
    };
    
    private static DOperator RUN = (List<dynamic> args, ref Context ctx) =>
    {
        var a = args[0];

        try
        {
            return Executor.Blocks.Find(x => x.Name == a.ToString()).Execute();
        }
        catch
        {
            return "^";
        }
    };
    
    private static DOperator AND = (List<dynamic> args, ref Context ctx) =>
    {
        return ctx.CurrentScope.Get(args[0]) && ctx.CurrentScope.Get(args[1]);
    };
    
    private static DOperator OR = (List<dynamic> args, ref Context ctx) =>
    {
        return ctx.CurrentScope.Get(args[0]) || ctx.CurrentScope.Get(args[1]);
    };
    
    private static DOperator MORE = (List<dynamic> args, ref Context ctx) =>
    {
        return ctx.CurrentScope.Get(args[0]) > ctx.CurrentScope.Get(args[1]);
    };
    
    private static DOperator MORE_EQ = (List<dynamic> args, ref Context ctx) =>
    {
        return ctx.CurrentScope.Get(args[0]) >= ctx.CurrentScope.Get(args[1]);
    };
    
    private static DOperator LESS = (List<dynamic> args, ref Context ctx) =>
    {
        return ctx.CurrentScope.Get(args[0]) < ctx.CurrentScope.Get(args[1]);
    };
    
    private static DOperator LESS_EQ = (List<dynamic> args, ref Context ctx) =>
    {
        return ctx.CurrentScope.Get(args[0]) <= ctx.CurrentScope.Get(args[1]);
    };
    
    private static DOperator EQUAL = (List<dynamic> args, ref Context ctx) =>
    {
        return ctx.CurrentScope.Get(args[0]).Equals(ctx.CurrentScope.Get(args[1]));
    };
    
    private static DOperator NOT_EQUAL = (List<dynamic> args, ref Context ctx) =>
    {
        return !(ctx.CurrentScope.Get(args[0]).Equals(ctx.CurrentScope.Get(args[1])));
    };
    
    private static DOperator NOT = (List<dynamic> args, ref Context ctx) =>
    {
        return !ctx.CurrentScope.Get(args[0]);
    };
    
    private static DOperator IN_OPERATOR = (List<dynamic> args, ref Context ctx) =>
    {
        ctx.CurrentScope.Put(args[0], Console.ReadLine() ?? "");
        return "^";
    };
    
    private static DOperator HAS_OPERATOR = (List<dynamic> args, ref Context ctx) =>
    {
        if (ctx.CurrentScope.Get(args[0]).GetType() == typeof(List<dynamic>))
        {
            return (ctx.CurrentScope.Get(args[0]) as List<dynamic>).Contains(ctx.CurrentScope.Get(args[1]));
        }
        else if (ctx.CurrentScope.Get(args[0]).GetType() == typeof(string))
        {
            return (ctx.CurrentScope.Get(args[0]) as string).Contains(ctx.CurrentScope.Get(args[1]).ToString());
        }
        return "^";
    };
    
    private static DOperator UNLOAD_OPERATOR = (List<dynamic> args, ref Context ctx) =>
    {
        if (args[0].GetType() == typeof(List<dynamic>))
        {
            List<dynamic> arg = args[0];
            foreach (var i in arg)
            {
                Block? block = Executor.Blocks.Find(x => x.Name.ToString() == i.ToString());
                if (block != null)
                {
                    Executor.Blocks.Remove(block);
                }
            }

            foreach (var i in arg)
            {
                if (Executor.Structs.ContainsKey(i.ToString()))
                {
                    Executor.Structs.Remove(i.ToString());
                }
            }
        }
        else
        {
            Block? block = Executor.Blocks.Find(x => x.Name.ToString() == args[0].ToString());
            if (block != null)
            {
                Executor.Blocks.Remove(block);
            }
            else
            {
                if (Executor.Structs.ContainsKey(args[0].ToString()))
                {
                    Executor.Structs.Remove(args[0].ToString());
                }
            }
        }
        return "^";
    };
    
    private static DOperator FORMAT_OPERATOR = (List<dynamic> args, ref Context ctx) =>
    {
        string a = args[0];
        List<dynamic> b;
        if (args[1].GetType() == typeof(List<dynamic>))
        {
            b = args[1];
        }
        else
        {
            b = [args[1]];
        }

        string MyFormat(string format, List<dynamic> args)
        {
            for (int i = 0; i < args.Count; i++)
            {
                try
                {
                    format = format.Replace("{" + i + "}", args[i].ToString());
                }
                catch
                {
                    break;
                }
            }

            return format;
        }
        
        return MyFormat(a, b);
    };
    
    private static DOperator IMPORT = (List<dynamic> args, ref Context ctx) =>
    {
        if (args[0].GetType() == typeof(List<dynamic>))
        {
            List<dynamic> a = args[0];
            foreach (var i in a)
            {
                ExecutorHelper.Import(ctx.CurrentScope.Get(i).ToString());
            }
        }
        else
        {
            ExecutorHelper.Import(ctx.CurrentScope.Get(args[0]).ToString());
        } 
        return "^";
    };
    
    private static DOperator DESTRUCT = (List<dynamic> args, ref Context ctx) =>
    {
        var @struct = ctx.CurrentScope.Get(args[0]);
        if (@struct.GetType() == typeof(Struct))
        {
            List<dynamic> allFields = [..(@struct as Struct).Fields.Values.ToList()];
            try
            {
                ctx.CurrentScope.Del(args[0]);
            }
            catch {}
            
            return allFields;
        }
        else
        {
            throw new Exception("NOT A STRUCTURE");
        }
    };
    
    private static DOperator DEL_OPERATOR = (List<dynamic> args, ref Context ctx) =>
    {
        if (args[0].GetType() == typeof(List<dynamic>))
        {
            foreach (var a in args[0] as List<dynamic>)
            {
                ctx.CurrentScope.Del(a);
            }
        }
        else
        {
            ctx.CurrentScope.Del(args[0]);
        }
        return "^";
    };
    
    private static DOperator DOT_OPERATOR = (List<dynamic> args, ref Context ctx) =>
    {
        var @struct = ctx.CurrentScope.Get(args[0]);
        if (@struct.GetType() != typeof(Struct))
        {
            throw new Exception("NOT A STRUCTURE");
        }
        @struct = (Struct)@struct;

        return (@struct as Struct)[args[1]];
    };
    
    private static DOperator INSTANCE_OPERATOR = (List<dynamic> args, ref Context ctx) =>
    {
        if (Executor.Structs.ContainsKey(args[0].ToString()))
        {
            var a = args[1] as List<dynamic>;
            string b = args[0].ToString();
            return Executor.Structs[b].Accept(a, ref ctx);
        }
        
        throw new Exception("STRUCTURE NOT FOUND");
    };
    
    private static DOperator CAST_OPERATOR = (List<dynamic> args, ref Context ctx) =>
    {
        if (Executor.Structs.ContainsKey(args[1].ToString()))
        {
            var sourceStruct = ctx.CurrentScope.Get(args[0]) as Struct;
            string b = args[1].ToString();

            var newStruct = new Struct(Executor.Structs[b]);

            foreach (var field in sourceStruct.Fields)
            {
                newStruct.TryAccept(field.Key.ToString(), field.Value.ToString());
            }
            newStruct.Done();
            return newStruct;
        }
        
        throw new Exception("STRUCTURE NOT FOUND");
    };
    
    
    
    private static DOperator RUNIF = (List<dynamic> args, ref Context ctx) =>
    {
        if (args[1].GetType() == typeof(List<dynamic>))
        {
            if (ctx.CurrentScope.Get(args[0]))
            {
                return Executor.Blocks.Find(x => x.Name == args[1][0].ToString()).Execute();   
            }
            else
            {
                return Executor.Blocks.Find(x => x.Name == args[1][1].ToString()).Execute();   
            }
        }
        else
        {
            if (ctx.CurrentScope.Get(args[0]))
            {
                return Executor.Blocks.Find(x => x.Name == args[1].ToString()).Execute();   
            }
        }

        return "^";
    };
    
    private static DOperator REM_AT = (List<dynamic> args, ref Context ctx) =>
    {
        var a = ctx.CurrentScope.Get(args[0]) as List<dynamic>;
        int b = Convert.ToInt32(ctx.CurrentScope.Get(args[1]));
        
        a.RemoveAt(b);
        
        return a;
    };
    
    private static DOperator INTERVAL_OPERATOR = (List<dynamic> args, ref Context ctx) =>
    {
        int a = Convert.ToInt32(ctx.CurrentScope.Get(args[0]));
        int b = Convert.ToInt32(ctx.CurrentScope.Get(args[1]));
        List<dynamic> c = new();

        for (int i = a; i <= b; i++)
        {
            c.Add(i);
        }
        
        return c;
    };
    
    private static DOperator REV_OPERATOR = (List<dynamic> args, ref Context ctx) =>
    {
        var value = args[0];
        
        var c = ctx.CurrentScope.variables.ToList().FindAll(x =>  x.Value.Value.Equals(value));
        

        if (c.Count == 0)
        {
            return value;
        }

        if (c.Count == 1)
        {
            return $"\"{c[0].Key}\"";
        }
        
        var rand = new Random();
        var d = c[rand.Next(c.Count - 1)].Key;
        
        return $"\"{d}\"";
    };
    
    private static DOperator AS_OPERATOR = (List<dynamic> args, ref Context ctx) =>
    {
        var @var = ctx.CurrentScope.Get(args[0]);
        var type = args[1];

        switch (type)
        {
            case "int":
                return Convert.ToInt32(@var);
            case "str":
                return @var.ToString();
            case "bool":
                return Convert.ToBoolean(@var);
        }

        return @var;
    };
    
    public static void Init()
    {
        Register("args", 0, ARGS_OP);
        Register("import", 0, IMPORT);
        Register("unload", 0, UNLOAD_OPERATOR);
        Register("return", 0, RETURN);
        Register(":", 1, RUN_WITH_ARGS);
        Register("map", 1, MAP_OP);
        Register("cast", 2, CAST_OPERATOR);
        Register("instance", 1, INSTANCE_OPERATOR);
        Register("run", 1, RUN);
        Register("=>", 1, RUN_FORWARD);
        Register("runif", 1, RUNIF);
        Register("=", 0, ASSIGN);
        Register("del", 0, DEL_OPERATOR);
        Register("->", 0, FORWARD);
        Register("+=", 0, ADD_ASSIGN);
        Register("]+", 0, ADD_TO);
        Register("]-", 0, REM_AT);
        Register("-=", 0, SUB_ASSIGN);
        Register("*=", 0, MUL_ASSIGN);
        Register("/=", 0, DIV_ASSIGN);
        Register("println", 0, PRINTLN);
        Register("outln", 0, PRINTLN);
        Register("out", 0, PRINT);
        Register("in", 0, IN_OPERATOR);
        Register("print", 0, PRINT);
        Register("==", 3, EQUAL);
        Register("!=", 3, NOT_EQUAL);
        Register(">=", 3, MORE_EQ);
        Register(">", 3, MORE);
        Register("!", 3, NOT);
        Register("&&", 2, AND);
        Register("||", 2, OR);
        Register("<=", 3, LESS_EQ);
        Register("<", 3, LESS);
        Register("jsonit", 4, JSONIT);
        Register(",", 5, GROUP);
        Register("has", 4, HAS_OPERATOR);
        Register("format", 4, FORMAT_OPERATOR);
        Register("do", 4, DO_OPERATOR);
        Register("..", 5, INTERVAL_OPERATOR);
        Register("`", 5, NAME_GROUP);
        Register("*", 6, MULTIPLY);
        Register("/", 6, DIVISION);
        Register("+", 7, ADDITION);
        Register("-", 7, SUB);
        Register("get", 11, GETVAR);
        Register("rev", 11, REV_OPERATOR);
        Register("destr", 10, DE_STR);
        Register("destruct", 10, DESTRUCT);
        Register("as", 9, AS_OPERATOR);
        Register("len", 9, LEN_OP);
        Register("at", 9, AT_OPERATOR);
        Register(".", 11, DOT_OPERATOR);
    }

    public static DOperator Logic(string name)
    {
        if(!_operators.TryGetValue(name, out var @operator)) throw new Exception($"Operator {name} not found");
        return @operator.op;
    }

    public static int Check(string name)
    {
        if(!_operators.TryGetValue(name, out var @operator)) throw new Exception($"Operator {name} not found");
        return @operator.priority;
    }

    private static void Register(string name, int priority, DOperator logic)
    {
        _operators[name] = new Operator { op = logic, priority = priority };
    } 
}

