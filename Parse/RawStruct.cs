using System.Text.Json;

namespace RorunLang.Parse;

public class RawStruct
{
    public List<Token> Fields { get; set; } = new();
    public string Name { get; set; } = "";
}

public class Struct
{
    public readonly List<string> FieldsNames = new();
    public string Type = "";
    public Dictionary<string, dynamic> Fields { get; set; } = new();
    int FieldCount { get; set; }
    
    public Struct(Struct t)
    {
        FieldCount = t.Fields.Count;
        FieldsNames = new List<string>(t.FieldsNames);
        Fields = new();
        Type = t.Type;
    }

    public Struct(List<string> fields, string type)
    {
        FieldsNames = [..fields];
        FieldCount = fields.Count;
        Type = type;
    }

    public void Done()
    {
        foreach(var field in FieldsNames)
        {
            if (!Fields.ContainsKey(field))
            {
                Fields[field] = 0;
            }
        }
    }

    public void TryAccept(string field, string value)
    {
        if (FieldsNames.Contains(field))
        {
            Fields[field] = value;
        }
    }
    
    public Struct Accept(List<dynamic> flds, ref Context ctx)
    {
        var a = new Struct(this);
        for (int i = 0; i < FieldCount; i++)
        {
            try
            {
                a.Fields[FieldsNames[i]] = ctx.CurrentScope.Get(flds[i]);
            }
            catch
            {
                a.Fields[FieldsNames[i]] = "";
            }
        }

        return a;
    }
    
    public dynamic this[dynamic index]
    {
        get
        {
            if (Fields.ContainsKey(index.ToString()))
            {
                return Fields[index];
            }
            Console.WriteLine(index.ToString());

            throw new Exception("Field not found");
        }
        set
        {
            if (Fields.ContainsKey(index.ToString()))
            {
                Fields[index] = value;
            }
            throw new Exception("Field not found");
        }
    }
}