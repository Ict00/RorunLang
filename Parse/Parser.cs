using RorunLang.Execution;

namespace RorunLang.Parse;

public static class Parser
{
    public static ParseInfo SecondStage(RawParseInfo parseInfo)
    {
        ParseInfo secondStageParseInfo = new ParseInfo();

        foreach (var block in parseInfo.Blocks)
        {
            var newBlock = new Block
            {
                Modifiers = [..block.Modifiers],
                Name = block.Name,
            };
            
            for (int i = 0; i < block.Body.Count; i++)
            {
                if (block.Body[i].Type != TokenType.EndLine)
                {
                    newBlock.Expressions.Add(ParseExpr(block.Body, ref i));
                }
            }
            secondStageParseInfo.Blocks.Add(newBlock);
        }

        foreach (var @struct in parseInfo.Structs)
        {
            List<string> newFields = new();
            foreach (var token in @struct.Fields)
            {
                if (token.Type == TokenType.Identifier)
                {
                    newFields.Add(token.Value);
                }
            }
            secondStageParseInfo.Structs.Add(@struct.Name, new Struct(newFields, @struct.Name));
        }
        
        return secondStageParseInfo;
    }

    private static bool Exceed<T>(ICollection<T> collection, int i)
    {
        if(collection.Count <= i) throw new Exception("No ; or something else idk");
        return false;
    }

    private static OpExpression ParseExpr(List<Token> tokens, ref int i)
    {
        OpExpression expression = new OpExpression();
        bool isBinary = tokens[i].Type != TokenType.Operator;

        string op = string.Empty;
        
        if (isBinary)
        {
            i++;
            op = tokens[i].Value;
            expression.@do = Operators.Logic(op);
            expression.lhs = SingleExpression.Of(tokens[i-1].Value);
            if (!Exceed(tokens, i + 1))
            {
                if (tokens[i + 1].Type == TokenType.Operator)
                {
                    i++;
                    expression.rhs = ParseExpr(tokens, ref i);
                }
                else if (tokens[i + 1].Type == TokenType.EndLine)
                {
                    throw new Exception("Expression expected");
                }
                else
                {
                    if (!Exceed(tokens, i + 2))
                    {
                        if (tokens[i + 2].Type == TokenType.Operator)
                        {
                            int otherPriority = Operators.Check(tokens[i+2].Value);
                            int currentPriority = Operators.Check(op);
                            if (otherPriority > currentPriority && 
                                (op == "+" ||
                                 op == "-" ||
                                 op == "/" ||
                                 op == "*"))
                            {
                                i++;
                                var newExpr = ParseExpr(tokens, ref i);
                                var lhs = newExpr.lhs;
                                expression.rhs = lhs;
                                newExpr.lhs = expression;
                                
                                return newExpr;
                            }
                            if ((otherPriority < currentPriority &&
                                !(tokens[i+2].Value == "==" || tokens[i+2].Value == "&&")) || 
                                op == "." && tokens[i+2].Value == ".")
                            {
                                i++;
                                var newExpr = ParseExpr(tokens, ref i);
                                var lhs = newExpr.lhs;
                                expression.rhs = lhs;
                                newExpr.lhs = expression;
                                
                                return newExpr;
                            }
                            if (tokens[i + 2].Value == "&&" || tokens[i + 2].Value == "||")
                            {
                                expression.rhs = SingleExpression.Of(tokens[i+1].Value);
                                i++;
                                var nexpr = ParseExpr(tokens, ref i);
                                nexpr.lhs = expression;
                                return nexpr;
                            }

                            i++;
                            expression.rhs = ParseExpr(tokens, ref i);
                            return expression;
                        }
                        expression.rhs = SingleExpression.Of(tokens[i + 1].Value);
                        i++;
                    }
                }
            }
        }
        else
        {
            op = tokens[i].Value;
            expression.@do = Operators.Logic(op);
            if (!Exceed(tokens, i + 1))
            {
                if (tokens[i + 1].Type == TokenType.Operator)
                {
                    i++;
                    expression.lhs = ParseExpr(tokens, ref i);
                }
                else if (tokens[i + 1].Type == TokenType.EndLine)
                {
                    throw new Exception("Expression expected");
                }
                else
                {
                    if (!Exceed(tokens, i + 2))
                    {
                        if (tokens[i + 2].Type == TokenType.Operator)
                        {
                            i++;
                            expression.lhs = ParseExpr(tokens, ref i);
                            return expression;
                        }
                        expression.lhs = SingleExpression.Of(tokens[i+1].Value);
                        i++;
                        return expression;
                    }
                    expression.lhs = SingleExpression.Of(tokens[i + 1].Value);
                }
            }
        }
        
        return expression;
    }
    
    public static RawParseInfo FirstStage(List<Token> tokens)
    {
        var inf = new RawParseInfo();
        List<string> modifiers = new();
        var nextName = "";
        var what = "";
        
        for(int i = 0; i < tokens.Count; i++)
        {
            var t = tokens[i];
            if (t.Type.Equals(TokenType.Modifier))
            {
                if (t.Value.Equals("block"))
                {
                    what = "block";
                    nextName = tokens[i+1].Value;
                }
                else if (t.Value.Equals("struct"))
                {
                    what = "struct";
                    nextName = tokens[i+1].Value;
                }
                else
                {
                    switch (t.Value)
                    {
                        case "open": modifiers.Add(t.Value);
                            break;
                        case "closed": goto case "open";
                        case "early": goto case "open";
                        case "late": goto case "open";
                    }
                }
            }
            else
            {
                if (what == "")
                {
                    nextName = t.Value;
                }
                else
                {
                    if (t.Type.Equals(TokenType.LB))
                    {
                        
                        var body = GetBody(tokens, "}", ref i);
                        inf.Blocks.Add(new RawBlock()
                        {
                            Name = nextName,
                            Body = body,
                            Modifiers = modifiers.Count > 0 ? new List<string>(modifiers) : ["late", "open"]
                        });
                        modifiers.Clear();
                        
                        nextName = "";
                    }
                    else if (t.Value.Equals("["))
                    {
                        var body = GetBody(tokens, "]", ref i);
                        inf.Structs.Add(new RawStruct()
                        {
                            Name = nextName,
                            Fields = body
                        });
                        modifiers.Clear();
                        nextName = "";
                    }
                }
            }
        }
        
        return inf;
    }

    private static List<Token> GetBody(List<Token> tokens, string stop_seq, ref int i)
    {
        i++;
        List<Token> ret = new();

        for (; i < tokens.Count; i++)
        {
            if (tokens[i].Value.Equals(stop_seq))
            {
                break;
            }
            ret.Add(tokens[i]);
        }
        
        return ret;
    }
}