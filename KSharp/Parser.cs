using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSharp
{
    class Parser
    {

        public static List<Line> ParseFile(string path)
        {
            
            var list = new List<Line>();
            string[] lines = File.ReadAllLines(path);

            int i = 0;
            int k = 0;
            foreach(var line in lines)
            {
                i++;
                var my_line = new Line();
                my_line.LineIndex = i;
                string[] exp = ExplodeLineString(line);

                List<Token> my_tokens = new List<Token>();
                foreach(string expression in exp)
                {
                    var x = expression.Trim();
                    string quote = '"'.ToString();
                    if (x == "(")
                    {
                        Token token = new Token(k, Token.TOKEN_TYPE.BREC_START, x, my_line);
                        
                        my_tokens.Add(token);
                    }
                    else if (x == ")")
                    {
                        Token token = new Token(k, Token.TOKEN_TYPE.BREC_END, x, my_line); ;
                        my_tokens.Add(token);
                    }
                    else if (x == "{")
                    {
                        Token token = new Token(k, Token.TOKEN_TYPE.CON_START, x, my_line);
                        my_tokens.Add(token);
                    }
                    else if (x == "}")
                    {
                        Token token = new Token(k, Token.TOKEN_TYPE.CON_END, x, my_line);
                        my_tokens.Add(token);
                    }
                    else if (x == quote)
                    {
                        Token token = new Token(k, Token.TOKEN_TYPE.QUOTE, x, my_line);
                        my_tokens.Add(token);
                    }
                    else if(x == "+")
                    {
                        Token token = new Token(k, Token.TOKEN_TYPE.PLUS, x, my_line);
                        my_tokens.Add(token);
                    }
                    else if (x == "-")
                    {
                        Token token = new Token(k, Token.TOKEN_TYPE.MINUS, x, my_line);
                        my_tokens.Add(token);
                    }
                    else if (x == "/")
                    {
                        Token token = new Token(k, Token.TOKEN_TYPE.DIVIDER, x, my_line);
                        my_tokens.Add(token);
                    }
                    else if (x == "*")
                    {
                        Token token = new Token(k, Token.TOKEN_TYPE.MULTIP, x, my_line);
                        my_tokens.Add(token);
                    }
                    else if(x == "m[")
                    {
                        Token token = new Token(k, Token.TOKEN_TYPE.MATH_START, x, my_line);
                        my_tokens.Add(token);
                    }
                    else if(x == "]m")
                    {
                        Token token = new Token(k, Token.TOKEN_TYPE.MATH_END, x, my_line);
                        my_tokens.Add(token);
                    }
                   
                    else if(k == 0)
                    {
                        if(x != "")
                        {
                            Token token = new Token(k, Token.TOKEN_TYPE.COMMAND, x, my_line);
                            my_tokens.Add(token);
                        }
                        else
                        {
                            k--;
                        }

                    }
                    else if(x == "")
                    {
                        k--;

                    }
                    else
                    {
                        if(x.Contains("$GL_"))
                        {
                            //GLOBAL VARIABLE
                            var value = x.Replace("$GL_", "");

                            Token token = new Token(k, Token.TOKEN_TYPE.GLOBAL, value, my_line);
                            my_tokens.Add(token);
                            
                            
                        }
                        else
                        {
                            if(x.Contains("$"))
                            {
                                //NORMAL VARIABLE
                                var value = x.Replace("$", "");


                                Token token = new Token(k, Token.TOKEN_TYPE.VARIABLE, value, my_line);
                                token.STATIC_VALUE = x;
                                my_tokens.Add(token);

                            }
                            else
                            {
                                Token token = new Token(k, Token.TOKEN_TYPE.NORMAL, x, my_line);
                                my_tokens.Add(token);
                            }


    
                        }

                    }

                    k++;
                }

                my_line.Tokens = my_tokens;
                list.Add(my_line);
                k = 0;
                
               
            }

            return list;
        }

        

        private static string[] ExplodeLineString(string line)
        {
            string quote = '"'.ToString();
            line = line.Trim();
            line = line.Replace(quote, $" {quote} ");
            line = line.Replace("(", " ( ");
            line = line.Replace(")", " ) ");
            line = line.Replace("(", " ( ");
            line = line.Replace("{", " { ");
            line = line.Replace("}", " } ");
            line = line.Replace("(", $" ( ");

            line = line.Replace("+", " + ");
            line = line.Replace("-", " - ");
            line = line.Replace("*", " * ");
            line = line.Replace("/", " / ");
            line = line.Replace("m[", " m[ ");
            line = line.Replace("]m", " ]m ");

            return line.Split(' ');
        }
    }
}
