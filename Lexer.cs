namespace RorunLang;

public static class Lexer
{
    private enum Writing
    {
        String,
        Comment,
        I
    }

    private static Token C(Token source)
    {
        if (Utils.Modifiers.Contains(source.Value))
        {
            source.Type = TokenType.Modifier;
        }
        if (source.Value.Equals("{"))
        {
            source.Type = TokenType.LB;
        }

        if (source.Value.Equals("}"))
        {
            source.Type = TokenType.RB;
        }

        if (Utils.MOperators.Contains(source.Value))
        {
            source.Type = TokenType.Operator;
        }
        return source;
    }

    public static List<Token> FixTokens(List<Token> inTokens)
    {
        List<Token> fixedTokens = new List<Token>();

        for (int i = 0; i < inTokens.Count; i++)
        {
            try
            {
                if (inTokens[i].Type != TokenType.String &&
                    inTokens[i + 1].Type != TokenType.String)
                {
                    if (Utils.MOperators.Contains(inTokens[i].Value + inTokens[i + 1].Value))
                    {
                        fixedTokens.Add(new Token
                        {
                            Type = TokenType.Operator,
                            Value = inTokens[i].Value+inTokens[i + 1].Value,
                        });
                        i += 1;
                        continue;
                    }
                }
                fixedTokens.Add(inTokens[i]);
                
            } catch {}
        }
        fixedTokens.Add(inTokens.Last());

        return fixedTokens;
    }

    private static string TransformStr(string str) =>
        str.Replace("\\x1b", "\x1b").Replace("\\\"", "\"").Replace("\\n", "\n").Replace("\\e", "\x1b");
    
    public static List<Token> GetTokens(string code)
    {
        var tokens = new List<Token>();
        string content = string.Empty;
        Writing writing = Writing.I;
        
        foreach (var ch in code)
        {
            switch (writing)
            {
                case Writing.I:
                    if (content.EndsWith("//"))
                    {
                        content = string.Empty;
                        writing = Writing.Comment;
                        break;
                    }
                    if (char.IsWhiteSpace(ch) || char.IsControl(ch))
                    {
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            tokens.Add(C(new Token
                            {
                                Type = TokenType.Identifier,
                                Value = content,
                            }));
                        }
                        content = string.Empty;
                        continue;
                    }

                    if (ch.Equals(';'))
                    {
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            tokens.Add(C(new Token
                            {
                                Type = TokenType.Identifier,
                                Value = content,
                            }));
                        }
                        tokens.Add(new Token
                        {
                            Type = TokenType.EndLine,
                            Value = ";",
                        });
                        content = string.Empty;
                        continue;
                    }
                    if (ch.Equals('"'))
                    {
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            tokens.Add(C(new Token
                            {
                                Type = TokenType.Identifier,
                                Value = content,
                            }));
                        }
                        content = string.Empty;
                        writing = Writing.String;
                        continue;
                    }

                    if (ch.Equals('{') || ch.Equals('}'))
                    {
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            tokens.Add(C(new Token
                            {
                                Type = TokenType.Identifier,
                                Value = content,
                            }));
                        }
                        tokens.Add(C(new Token
                        {
                            Type = TokenType.Identifier,
                            Value = ch.ToString(),
                        }));
                        content = string.Empty;
                        continue;
                    }

                    if (Utils.Operators.Contains(ch))
                    {
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            tokens.Add(new Token
                            {
                                Type = TokenType.Identifier,
                                Value = content,
                            });
                            content = string.Empty;
                        }
                        tokens.Add(new Token
                        {
                            Type = TokenType.Operator,
                            Value = ch.ToString()
                        });
                    }
                    else
                    {
                        if (ch != '\n' && ch != '\r')
                        {
                            content += ch;
                        }
                    }
                    break;
                case Writing.String:
                    if (ch == '"')
                    {
                        if (!content.EndsWith("\\"))
                        {
                            writing = Writing.I;
                            tokens.Add(new Token { Type = TokenType.String, Value = TransformStr(content), });
                            content = string.Empty;
                        }
                        else
                        {
                            content += ch;
                        }
                    }
                    else
                    {
                        content += ch;
                    }
                    break;
                case Writing.Comment:
                    if (ch.Equals('\n'))
                    {
                        writing = Writing.I;
                    }
                    break;
            }
        }
        if (!string.IsNullOrWhiteSpace(content))
        {
            tokens.Add(C(new Token
            {
                Type = TokenType.Identifier,
                Value = content,
            }));
        }

        return tokens;
    }
}