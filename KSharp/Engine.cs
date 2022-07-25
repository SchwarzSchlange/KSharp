using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
                if (global.VALUE == "TIME")
                {
                    global.VALUE = TimeZone.CurrentTimeZone.ToLocalTime(DateTime.Now).ToLongTimeString();
                }
                else if(global.VALUE == "DIRECTORY")
                {
                    global.VALUE = System.IO.Directory.GetCurrentDirectory();
                }
                else if(global.VALUE == "VER")
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
                if (Engine.TryGetVariable(variable.VALUE, out object _out))
                {
                    variable.VALUE = _out.ToString();
                }
                else
                {
                    Debug.Warning($"At line {variable.Root.LineIndex} the variable '{variable.VALUE}' is not assinged.");
                    variable.TYPE = Token.TOKEN_TYPE.NULL;
                    variable.VALUE = "NULL";
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
                        Debug.Success($"[{name_token.VALUE}] attached as [{to_attach_value}]");
                    }
                    else
                    {
                        Debug.Error($"Name of the variable could not be found at line {tokens[0].Root.LineIndex}");
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
                        Debug.Success("Loop Count = " + LoopCount.ToString());
                        var runLines = LinesBetweenType(Program.LastLines, tokens[0].Root.LineIndex,Token.TOKEN_TYPE.CON_START, Token.TOKEN_TYPE.CON_END);

                        
                        for(int i = 1; i < LoopCount;i++)
                        {
                            foreach (var runLine in runLines)
                            {
                                RunTokens(runLine.Tokens);
                                //Thread.Sleep(1);
                            }
                        }
                        
                        /*
                        foreach (var runLine in runLines)
                        {
                            runLine.Tokens.Clear();
                        }
                        */
                        
                        

                    }
                    else
                    {
                        Debug.Error($"Unexpected loop count at line {tokens[0].Root.LineIndex}");
                        return;
                    }
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
                Debug.Success(Start.LineIndex.ToString());
                Line End = lines.FindLast(x => x.Tokens.Count > 0 && x.Tokens[0].TYPE == type2 && x.LineIndex > Start.LineIndex);
                Debug.Success(End.LineIndex.ToString());

                var between_lines = lines.FindAll(x => x.LineIndex > Start.LineIndex && x.LineIndex < End.LineIndex);
                return between_lines;
            
        

        }

        private static string StringGetValueBetweenType(List<Token> tokens,Token.TOKEN_TYPE type1, Token.TOKEN_TYPE type2)
        {
            string TO_RETURN = "";

            try
            {
                var first = tokens.Find(x => x.TYPE == type1);
                var last = tokens.FindLast(x => x.TYPE == type2);

                for(int i = first.INDEX+1;i < last.INDEX;i++)
                {
                    if(TO_RETURN == "")
                    {
                        TO_RETURN = tokens[i].VALUE;
                    }
                    else
                    {
                        TO_RETURN += " " + tokens[i].VALUE;
                    }
                    
                }
            }
            catch(Exception ex)
            {
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
