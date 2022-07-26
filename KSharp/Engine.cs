using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using org.mariuszgromada.math.mxparser;

namespace KSharp
{
    class Engine
    {
        private static Dictionary<string, object> Variables = new Dictionary<string, object>();


        public static void AddVariable(string Key,object Value)
        {
            if(Variables.ContainsKey(Key))
            {
                Variables[Key] = Value;
            }
            else
            {
                Variables.Add(Key, Value);
            }

        }

        public static bool TryGetVariable(string Key,out object _out)
        {
            if(!Variables.ContainsKey(Key))
            {
                _out = null;
                return false;
            }
            else
            {
                _out = Variables[Key];
                return true;
            }

        }

        public static bool TryGetTokenAtIndex(List<Token> tokens,int index,out Token _token)
        {
            try
            {
                var tocken = tokens.Find(x => x.INDEX == index);
                if (tocken != null)
                {
                    _token = tocken;
                   
                }
                else
                {
                    _token = null;
                }
                        
                
            }
            catch
            {
                _token = null;
                return false;
            }


            return true;
        }

        public static void ConvertAllGlobals(List<Token> tokens)
        {
            var globals = tokens.FindAll(x => x.TYPE == Token.TOKEN_TYPE.GLOBAL);

            foreach (var global in globals)
            {
               
                if (global.STATIC_VALUE == "TIME")
                {
                    global.VALUE = TimeZone.CurrentTimeZone.ToLocalTime(DateTime.Now).ToLongTimeString();
                }
                else if(global.STATIC_VALUE == "DIRECTORY")
                {
                    global.VALUE = System.IO.Directory.GetCurrentDirectory();
                }
                else if(global.STATIC_VALUE == "VER")
                {
                    global.VALUE = Program.VERSION;
                }
            
                else
                {
                    Debug.Warning($"At line {global.Root.LineIndex} the global '{global.VALUE}' is not defined");
                    global.TYPE = Token.TOKEN_TYPE.NULL;
                    global.VALUE = "NULL";
                }

            }

        }

        public static void ConvertAllVariables(List<Token> tokens)
        {
            var variables = tokens.FindAll(x => x.TYPE == Token.TOKEN_TYPE.VARIABLE);

            foreach (var variable in variables)
            {
                //Debug.Info($"Variable = {variable.VALUE} {variable.STATIC_VALUE}");
                if (Engine.TryGetVariable(variable.STATIC_VALUE.Replace("$",""), out object _out))
                {
                    variable.VALUE = _out.ToString();
                }
                else
                {
                    Debug.Warning($"At line {variable.Root.LineIndex} the variable '{variable.STATIC_VALUE}' is not assinged.");
                    variable.TYPE = Token.TOKEN_TYPE.NULL;
                    variable.VALUE = "NULL";
                }

            }

            //DebugLogTokens(tokens);

        }

        public static void DebugLogTokens(List<Token> tokens)
        {
            foreach (Token token in tokens)
            {
                Console.WriteLine($"[{token.INDEX}] => {token.TYPE} => {token.Root.LineIndex} => {token.VALUE}");
            }
        }

        public static List<Token> NormalizeTokenList(List<Token> tokens)
        {
            int i = 0;
            foreach(Token token in tokens)
            {
                token.INDEX = i;

                i++;
            }


            return tokens;
        }

        public static void ConvertAllMath(List<Token> tokens)
        {
            //ConvertAllVariables(tokens);
            if (tokens.Count > 0)
            {
                string expression = StringGetValueBetweenType(tokens, Token.TOKEN_TYPE.MATH_START, Token.TOKEN_TYPE.MATH_END,false);
                string static_expression = StringGetValueBetweenType(tokens, Token.TOKEN_TYPE.MATH_START, Token.TOKEN_TYPE.MATH_END, true);

                if (expression != null)
                {
                    //expression = expression.Replace(" ", "");

                    Debug.Success("EXPRESSION = " + expression);
                    Expression losung = new Expression(expression);
                    var calc = losung.calculate();
                    //Console.WriteLine(calc);

                    Token start = tokens.Find(x => x.TYPE == Token.TOKEN_TYPE.MATH_START);
                    Token end = tokens.Find(x => x.TYPE == Token.TOKEN_TYPE.MATH_END);


                    int count = end.INDEX - start.INDEX + 1;
                    //Debug.Info(count.ToString());
                    
                    tokens.RemoveRange(start.INDEX, count);
                    int new_index = int.Parse(Math.Floor(double.Parse((start.INDEX).ToString())).ToString());
                    Token to_add = new Token(new_index, Token.TOKEN_TYPE.MATH_TO_UPDATE, calc.ToString(), start.Root);

         

                    to_add.STATIC_VALUE = static_expression;
                    
                    
                    tokens.Insert(new_index, to_add);
                    tokens = NormalizeTokenList(tokens);
                    
                    //DebugLogTokens(tokens);
                }
                else if(tokens.Find(x => x.TYPE == Token.TOKEN_TYPE.MATH_TO_UPDATE) != null)
                {
                    Token update_token = tokens.Find(x => x.TYPE == Token.TOKEN_TYPE.MATH_TO_UPDATE);
                    //Console.WriteLine("STATIC VALUE AT MATH UPDATE : " +update_token.STATIC_VALUE);

                    string[] exploded = update_token.STATIC_VALUE.Split(' ');

                    string to_static_express = "";

                    foreach (var lan in exploded)
                    {
                        //Console.WriteLine("LAN = " + lan);
                        if (lan.Contains("$"))
                        {
                            string to_express = lan.Replace("$", "");

                            if(TryGetVariable(to_express,out object _value))
                            {
                                to_static_express += _value + " ";
                            }
                            else
                            {
                                to_static_express += "NULL";
                            }
                            
                            
                        }
                        else
                        {
                            to_static_express += lan + " ";
                        }
                    }
                    //Console.WriteLine("STATIC CONVERT : " + to_static_express);


                    //Debug.Success("STATIC EXPRESSION = " + to_static_express);
                    Expression losung = new Expression(to_static_express);
                    var calc = losung.calculate();


                    update_token.VALUE = calc.ToString();
                    //DebugLogTokens(tokens);

                }


              

            }


            

        }

        public static void RunTokens(List<Token> tokens)
        {
            if (tokens.Count != 0)
            {
                if (tokens[0].VALUE == "echo")
                {
                    Console.WriteLine(StringGetValueBetweenType(tokens, Token.TOKEN_TYPE.BREC_START, Token.TOKEN_TYPE.BREC_END));
                }
                else if (tokens[0].VALUE == "push")
                {

                    if (TryGetTokenAtIndex(tokens, 1, out Token name_token))
                    {
                        var to_attach_value = StringGetValueBetweenType(tokens, Token.TOKEN_TYPE.BREC_START, Token.TOKEN_TYPE.BREC_END);
                      
                        Engine.AddVariable(name_token.VALUE, to_attach_value);
                        Debug.Success($"[{name_token.STATIC_VALUE}] attached as [{to_attach_value}]");
                    }
                    else
                    {
                        Debug.Error($"Name of the variable could not be found at line {tokens[0].Root.LineIndex}");
                    }

 
                }
                else if(tokens[0].VALUE == "delay")
                {
                    if (TryGetTokenAtIndex(tokens, 1, out Token dur_token))
                    {
                        Debug.Success($"Waiting for [{dur_token.VALUE}]");
                        Thread.Sleep(int.Parse(dur_token.VALUE));
                    }
                    else
                    {
                        Debug.Error($"time duration not be found at line {tokens[0].Root.LineIndex}");
                    }
                }
                else if (tokens[0].VALUE == "DEBUG")
                {
                    if (TryGetTokenAtIndex(tokens, 1, out Token status_token))
                    {
                        if (status_token.VALUE == "ON")
                        {
                            Debug.IsDebug = true;
                        }
                        else if (status_token.VALUE == "OFF")
                        {
                            Debug.IsDebug = false;
                        }
                        else
                        {
                            Debug.Error($"Unknown status type for 'DEBUG' at line {tokens[0].Root.LineIndex} with value '{status_token.VALUE}'");
                            return;
                        }
                        Debug.Success($"DEBUG Mode is now : {status_token.VALUE}");
                    }
                    else
                    {
                        Debug.Error($"State could not be found at line {tokens[0].Root.LineIndex}");
                    }
                }
                else if (tokens[0].VALUE == "userinput")
                {
                    if (TryGetTokenAtIndex(tokens, 1, out Token name_token))
                    {
                        Console.Write(">>>");
                        string value = Console.ReadLine();
                        Engine.AddVariable(name_token.VALUE, value);
                        Debug.Success($"[{name_token.VALUE}] attached as [{value}] with user input.");
                    }
                    else
                    {
                        Debug.Error($"Name of the variable could not be found at line {tokens[0].Root.LineIndex}");
                    }

                }
                else if(tokens[0].VALUE == "loop")
                {
                    
                    if(int.TryParse(StringGetValueBetweenType(tokens, Token.TOKEN_TYPE.BREC_START, Token.TOKEN_TYPE.BREC_END),out int LoopCount))
                    {
                        //Debug.Success("Loop Count = " + LoopCount.ToString());
                        var runLines = LinesBetweenType(Program.LastLines, tokens[0].Root.LineIndex,Token.TOKEN_TYPE.CON_START, Token.TOKEN_TYPE.CON_END);

                        
                        for(int i = 1; i < LoopCount;i++)
                        {
                            foreach (var runLine in runLines)
                            {
                                Engine.ConvertAllGlobals(runLine.Tokens);
                                Engine.ConvertAllVariables(runLine.Tokens);
                                Engine.ConvertAllMath(runLine.Tokens);
                                RunTokens(runLine.Tokens);
                     

                            }
                        }
                    }
                    else
                    {
                        Debug.Error($"Unexpected loop count at line {tokens[0].Root.LineIndex}");
                        return;
                    }
                }
                else if(tokens[0].VALUE == "clear")
                {
                    Console.Clear();
                }
                else
                {
                    if (tokens[0].VALUE != "")
                    {
                        if(tokens[0].TYPE != Token.TOKEN_TYPE.CON_START && tokens[0].TYPE != Token.TOKEN_TYPE.CON_END)
                        {
                            Debug.Error($"Unknown Command : '{tokens[0].VALUE}' at line {tokens[0].Root.LineIndex}");

                        }

                    }
                        
                }
            }

   
        }

        private static List<Line> LinesBetweenType(List<Line> lines, int start_index,Token.TOKEN_TYPE type1, Token.TOKEN_TYPE type2)
        {
            
                Line Start = lines.Find(x => x.Tokens.Count > 0 && x.Tokens[0].TYPE == type1 && x.LineIndex >start_index);
                //Debug.Success(Start.LineIndex.ToString());
                Line End = lines.FindLast(x => x.Tokens.Count > 0 && x.Tokens[0].TYPE == type2 && x.LineIndex > Start.LineIndex);
                //Debug.Success(End.LineIndex.ToString());

                var between_lines = lines.FindAll(x => x.LineIndex > Start.LineIndex && x.LineIndex < End.LineIndex);
                return between_lines;
            
        

        }

        private static string StringGetValueBetweenType(List<Token> tokens,Token.TOKEN_TYPE type1, Token.TOKEN_TYPE type2,bool returnStatics = false)
        {
            string TO_RETURN = "";

            try
            {
                var first = tokens.Find(x => x.TYPE == type1);
                var last = tokens.FindLast(x => x.TYPE == type2);

                if(first != null && last != null)
                {
                    for (int i = first.INDEX + 1; i < last.INDEX; i++)
                    {
                        if(returnStatics == false)
                        {
                            if (TO_RETURN == "")
                            {
                                TO_RETURN = tokens[i].VALUE;
                            }
                            else
                            {
                                TO_RETURN += " " + tokens[i].VALUE;
                            
                            }
                        }
                        else
                        {
                            if (TO_RETURN == "")
                            {
                                TO_RETURN = tokens[i].STATIC_VALUE;
                            }
                            else
                            {
                                TO_RETURN += " " + tokens[i].STATIC_VALUE;
                            }
                        }


                    }
                }
                else
                {
                    return null;
                }

            
            }
            catch(Exception ex)
            {
                Debug.Error(ex.Message);
                if(ex == default(ArgumentNullException))
                {
                    Debug.Error($"At line {tokens[0].Root.LineIndex} could not find the type : {type1} or {type2}");
                }
              
                return null;

            }

            return TO_RETURN;
            


        }
    }
}
