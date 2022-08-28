using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using org.mariuszgromada.math.mxparser;


namespace KSharp
{
    public class Engine
    {
        private static Dictionary<string, object> Variables = new Dictionary<string, object>();

        private static List<Block> Blocks = new List<Block>();
        private static List<Karray> Arrays = new List<Karray>();
        private static List<Window> Windows = new List<Window>();

        private static int TotalAddedLineCount = 0;

        public static void AddOverideBlock(string Name,string[] Parameters,List<Line> Lines)
        {
            Block block = Blocks.Find(x => x.Name == Name);

            if(block != null)
            {
                block.Name = Name;
                block.Lines = Lines;
                block.Paramaters = Parameters;
                Debug.Success($"'{Name}' is overriden");
            }
            else
            {
                Blocks.Add(new Block(Name, Lines, Parameters));
                Debug.Success($"'{Name}' is added");
            }
        }

        public static bool TryGetBlock(string Name,out Block _block)
        {
            Block block = Blocks.Find(x => x.Name == Name);

            if(block != null)
            {

                _block = block;
                return true;
            }
            else
            {
                _block = null;
                return false;
            }

  
        }


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

        public static void RemoveVariable(string Key)
        {
            Variables.Remove(Key);
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
                    return false;
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
                else if (global.STATIC_VALUE == "DATE")
                {
                    global.VALUE = TimeZone.CurrentTimeZone.ToLocalTime(DateTime.Now).ToLongDateString();
                }
                else if(global.STATIC_VALUE == "VER")
                {
                    global.VALUE = Program.VERSION;
                }           
                else if(global.STATIC_VALUE == "ARRAY")
                {
                    //DebugLogTokens(tokens);
                    string ArrayText = StringGetValueBetweenType(tokens, Token.TOKEN_TYPE.BREC_START, Token.TOKEN_TYPE.BREC_END, false, global.UNION);
                  
                    if(ArrayText == null)
                    {
                        Debug.Error($"Could not get the array parameter at line {global.Root.LineIndex}");
                    }
                    
                    string[] exploded = ArrayText.Split(',');
                    exploded[0] = exploded[0].Trim();

                    if(exploded.Length < 2)
                    {
                        Debug.Error($"Please give all the array parameters at line {global.Root.LineIndex}");
                    }

                    Karray thisArray = Arrays.Find(x => x.Name == exploded[0]);

                    if(thisArray == null)
                    {
                        Debug.Error($"Could not find the array with name '{exploded[0]}' at line '{global.Root.LineIndex}'");
                    }

                    //echo($GL_ARRAY(users,0))

                    object FetchedValue = null;
                    try
                    {
                       FetchedValue = thisArray.Content[int.Parse(exploded[1])];

                    }
                    catch
                    {
                        Debug.Error($"Given index ({exploded[1]}) is not avaible to array '{exploded[0]}' at line '{global.Root.LineIndex}'");
                    }

                    var VisibleTokens = TokensGetBetweenType(tokens, Token.TOKEN_TYPE.BREC_START, Token.TOKEN_TYPE.BREC_END, global.UNION);


                    var tok = tokens.FindAll(x => x.INDEX > global.INDEX && x.INDEX <= VisibleTokens.Last().INDEX+1);

                    tok.ForEach(x => x.isVisible = false);

               
                  

                    /*
                    tokens.RemoveRange(global.INDEX, RemoveTokens.Last().INDEX+1 - global.INDEX+1);
                    
                    Token toAdd = new Token(global.INDEX, Token.TOKEN_TYPE.GLOBAL, FetchedValue.ToString(), global.Root, global.LAYER, global.UNION);
                    toAdd.STATIC_VALUE = global.STATIC_VALUE;
                    tokens.Insert(global.INDEX,toAdd);
                                        tokens = NormalizeTokenList(tokens);
                     */

                    global.VALUE = FetchedValue.ToString();


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
                  
                    Debug.Warning($"At line {variable.Root.LineIndex} the variable '{variable.STATIC_VALUE} | {variable.VALUE}' is not assinged.");
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
                Console.WriteLine($"[{token.INDEX}][{token.isVisible}] => {token.TYPE} => {token.Root.LineIndex} => {token.VALUE}[{token.STATIC_VALUE}]");
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
                    expression = expression.Replace(" ", "");
              
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

                    to_static_express = to_static_express.Replace(" ", "");
                    //Debug.Success("STATIC EXPRESSION = " + to_static_express);
                    Expression losung = new Expression(to_static_express);
                    var calc = losung.calculate();


                    update_token.VALUE = calc.ToString();
                    //DebugLogTokens(tokens);

                }


              

            }


            

        }

        static bool isBreak = false;

        public static void RunTokens(Line RunningLine, List<Token> tokens, bool calledInLoop = false, bool calledInBlock = false)
        {

            if (RunningLine.isGonnaPassed)
            {
                RunningLine.isGonnaPassed = false;
                Program.CurrentReadingLine++;
                return;
            }

            if (tokens.Count != 0)
            {
                if (tokens[0].VALUE == "echo")
                {
                    Console.WriteLine(StringGetValueBetweenType(tokens, Token.TOKEN_TYPE.BREC_START, Token.TOKEN_TYPE.BREC_END, false, 0, true));
                }
                else if (tokens[0].VALUE == "TITLE")
                {
                    var title = StringGetValueBetweenType(tokens, Token.TOKEN_TYPE.BREC_START, Token.TOKEN_TYPE.BREC_END, false, 0);

                    Console.Title = title;

                }
                else if (tokens[0].VALUE == "INCLUDE")
                {
                    var path = StringGetValueBetweenType(tokens, Token.TOKEN_TYPE.BREC_START, Token.TOKEN_TYPE.BREC_END, false, 0);

                    if (path == null)
                    {
                        Debug.Error("Please give a path at line " + RunningLine.LineIndex);
                    }

                    if (calledInBlock)
                    {
                        Debug.Error("'INCLUDE' could not be called from a block at line : " + RunningLine.LineIndex);
                    }
                    else if (calledInLoop)
                    {
                        Debug.Error("'INCLUDE' could not be called from a loop at line : " + RunningLine.LineIndex);
                    }

                    var parsedIncludeLines = Parser.ParseFile(path);
                    TotalAddedLineCount += parsedIncludeLines.Count;



                    Program.LastLines.InsertRange(RunningLine.LineIndex, parsedIncludeLines);

                    NormalizeLines(Program.LastLines);

                }
                else if (tokens[0].VALUE == "echoline")
                {
                    Console.Write(StringGetValueBetweenType(tokens, Token.TOKEN_TYPE.BREC_START, Token.TOKEN_TYPE.BREC_END, false, 0, true));
                }
                else if (tokens[0].VALUE == "push")
                {

                    if (TryGetTokenAtIndex(tokens, 1, out Token name_token))
                    {
                        if (Char.IsDigit(name_token.VALUE[0]))
                        {
                            Debug.Error($"A variable could not start with a digit. At line {tokens[0].Root.LineIndex}");
                            return;
                        }
                        var to_attach_value = StringGetValueBetweenType(tokens, Token.TOKEN_TYPE.BREC_START, Token.TOKEN_TYPE.BREC_END);

                        AddVariable(name_token.VALUE, to_attach_value);
                        Debug.Success($"[{name_token.STATIC_VALUE}] attached as [{to_attach_value}]");
                    }
                    else
                    {
                        Debug.Error($"Name of the variable could not be found at line {tokens[0].Root.LineIndex}");
                    }


                }
                else if (tokens[0].VALUE == "WAIT")
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
                        else if (status_token.VALUE == "DEVELOPER_MODE")
                        {
                            Debug.IsDeveloperMode = true;
                        }
                        else if (status_token.VALUE == "OFF")
                        {
                            Debug.IsDebug = false;
                        }
                        else
                        {
                            Debug.Error($"Unknown status type for 'DEBUG' at line {tokens[0].Root.LineIndex} with value '{status_token.VALUE}'");
                        }
                        Debug.Success($"DEBUG Mode is now : {status_token.VALUE}");
                    }
                    else
                    {
                        Debug.Error($"Debug state could not be found at line {tokens[0].Root.LineIndex}. Use 'ON' or 'OFF'");
                    }
                }
                else if (tokens[0].VALUE == "LROM")
                {
                    if (TryGetTokenAtIndex(tokens, 1, out Token delay_token))
                    {
                        if (int.TryParse(delay_token.VALUE, out int delay))
                        {
                            Program.LINE_RUN_DELAY = delay;
                            Debug.Success($"Run offset set to '{delay_token.VALUE}'");
                        }
                        else
                        {
                            Debug.Error($"Please give a float value to offset at line {delay_token.Root.LineIndex}. Given value : '{delay_token.VALUE}'");
                        }

                    }
                    else
                    {
                        Debug.Error($"Offset could not be found at line {tokens[0].Root.LineIndex}. Use a number");
                    }
                }
                else if (tokens[0].VALUE == "userinput")
                {
                    if (TryGetTokenAtIndex(tokens, 1, out Token name_token))
                    {
                        Console.Write($"[@{Environment.UserName}] >>> ");
                        string value = Console.ReadLine();
                        Engine.AddVariable(name_token.VALUE, value);
                        Debug.Success($"[{name_token.VALUE}] attached as [{value}] with 'USER INPUT'");
                    }
                    else
                    {
                        Debug.Error($"Name of the variable could not be found at line {tokens[0].Root.LineIndex}");
                    }

                }
                else if (tokens[0].VALUE == "loop")
                {

                    if (int.TryParse(StringGetValueBetweenType(tokens, Token.TOKEN_TYPE.BREC_START, Token.TOKEN_TYPE.BREC_END), out int LoopCount))
                    {
                        //Debug.Success("Loop Count = " + LoopCount.ToString());
                        var runLines = LinesBetweenType(Program.LastLines, tokens[0].Root.LineIndex, Token.TOKEN_TYPE.CON_START, Token.TOKEN_TYPE.CON_END);


                        for (int i = 1; i <= LoopCount; i++)
                        {
                            for (Program.CurrentReadingLine = runLines.First().LineIndex - 1; Program.CurrentReadingLine < runLines.Last().LineIndex; Program.CurrentReadingLine++)
                            {

                                if (isBreak)
                                {
                                    Debug.Info("Breaking the inner loop.");
                                    break;
                                }
                                Engine.ConvertAllGlobals(Program.LastLines[Program.CurrentReadingLine].Tokens);
                                Engine.ConvertAllVariables(Program.LastLines[Program.CurrentReadingLine].Tokens);
                                Engine.ConvertAllMath(Program.LastLines[Program.CurrentReadingLine].Tokens);
                                RunTokens(Program.LastLines[Program.CurrentReadingLine], Program.LastLines[Program.CurrentReadingLine].Tokens, true);


                            }

                            if (isBreak)
                            {
                                Debug.Info("Breaking the outer loop.");
                                isBreak = false;
                                break;
                            }
                        }

                        Program.CurrentReadingLine = runLines.Last().LineIndex - 1;
                    }
                    else
                    {
                        Debug.Error($"Unexpected loop count at line {tokens[0].Root.LineIndex}");
                        return;
                    }
                }
                else if (tokens[0].VALUE == "CLEARSC")
                {
                    Console.Clear();
                }
                else if (tokens[0].VALUE == "if")
                {
                    //if(30 == 30) { }

                    var contidionTokens = TokensGetBetweenType(tokens, Token.TOKEN_TYPE.BREC_START, Token.TOKEN_TYPE.BREC_END);

                    var runLines = LinesBetweenType(Program.LastLines, contidionTokens[0].Root.LineIndex, Token.TOKEN_TYPE.CON_START, Token.TOKEN_TYPE.CON_END);

                    var elseLines = LinesBetweenType(Program.LastLines, runLines.Last().LineIndex, Token.TOKEN_TYPE.CON_START, Token.TOKEN_TYPE.CON_END);

                    bool isElse = false;
                    if (elseLines != null)
                    {
                        isElse = true;
                    }





                    Token EqualityToken = contidionTokens.Find(x => x.TYPE == Token.TOKEN_TYPE.CONDITION);

                    if (EqualityToken == null)
                    {
                        Debug.Error("Condition could not be found. At line " + contidionTokens[0].Root.LineIndex);

                    }

                    if (TryGetTokenAtIndex(tokens, EqualityToken.INDEX - 1, out Token LeftToken))
                    {
                        if (TryGetTokenAtIndex(tokens, EqualityToken.INDEX + 1, out Token RightToken))
                        {
                            if (EqualityToken.VALUE == "==")
                            {
                                //Console.WriteLine($"[{LeftToken.VALUE}] ?= [{RightToken.VALUE}]");
                                if (LeftToken.VALUE == RightToken.VALUE)
                                {
                                    Debug.Success("TRUE");


                                    if (isElse)
                                    {
                                        foreach (var ElseLine in elseLines)
                                        {
                                            ElseLine.isGonnaPassed = true;
                                        }
                                    }



                                }
                                else
                                {
                                    Debug.Info("FALSE");
                                    if (isElse)
                                    {
                                        Program.CurrentReadingLine = elseLines.First().LineIndex - 2;
                                    }
                                    else
                                    {

                                        Program.CurrentReadingLine = runLines.Last().LineIndex - 1;
                                    }

                                }
                            }
                            else if (EqualityToken.VALUE == ">>")
                            {
                                try
                                {
                                    if (float.Parse(LeftToken.VALUE) > float.Parse(RightToken.VALUE))
                                    {
                                        Debug.Success("TRUE");


                                        if (isElse)
                                        {
                                            foreach (var ElseLine in elseLines)
                                            {
                                                ElseLine.isGonnaPassed = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Debug.Warning("FALSE");
                                        if (isElse)
                                        {
                                            Program.CurrentReadingLine = elseLines.First().LineIndex - 2;
                                        }
                                        else
                                        {

                                            Program.CurrentReadingLine = runLines.Last().LineIndex - 1;
                                        }
                                    }
                                }
                                catch
                                {
                                    Debug.Error("Can not use that operator in if condition with un-float values at line " + EqualityToken.Root.LineIndex);
                                }

                            }
                            else if (EqualityToken.VALUE == "<<")
                            {
                                try
                                {
                                    if (float.Parse(LeftToken.VALUE) < float.Parse(RightToken.VALUE))
                                    {
                                        Debug.Success("TRUE");


                                        if (isElse)
                                        {
                                            foreach (var ElseLine in elseLines)
                                            {
                                                ElseLine.isGonnaPassed = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Debug.Warning("FALSE");
                                        if (isElse)
                                        {
                                            Program.CurrentReadingLine = elseLines.First().LineIndex - 2;
                                        }
                                        else
                                        {

                                            Program.CurrentReadingLine = runLines.Last().LineIndex - 1;
                                        }
                                    }
                                }
                                catch
                                {
                                    Debug.Error("Can not use that operator in if condition with un-float values at line " + EqualityToken.Root.LineIndex);
                                }
                            }
                            else if (EqualityToken.VALUE == ">=")
                            {
                                try
                                {
                                    if (float.Parse(LeftToken.VALUE) >= float.Parse(RightToken.VALUE))
                                    {
                                        Debug.Success("TRUE");


                                        if (isElse)
                                        {
                                            foreach (var ElseLine in elseLines)
                                            {
                                                ElseLine.isGonnaPassed = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Debug.Warning("FALSE");
                                        if (isElse)
                                        {
                                            Program.CurrentReadingLine = elseLines.First().LineIndex - 2;
                                        }
                                        else
                                        {

                                            Program.CurrentReadingLine = runLines.Last().LineIndex - 1;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.Info(ex.Message);
                                    Debug.Error("Can not use that operator in if condition with un-float values at line " + EqualityToken.Root.LineIndex);
                                }
                            }
                            else if (EqualityToken.VALUE == "<=")
                            {
                                try
                                {
                                    if (float.Parse(LeftToken.VALUE) <= float.Parse(RightToken.VALUE))
                                    {
                                        Debug.Success("TRUE");


                                        if (isElse)
                                        {
                                            foreach (var ElseLine in elseLines)
                                            {
                                                ElseLine.isGonnaPassed = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Debug.Warning("FALSE");
                                        if (isElse)
                                        {
                                            Program.CurrentReadingLine = elseLines.First().LineIndex - 2;
                                        }
                                        else
                                        {

                                            Program.CurrentReadingLine = runLines.Last().LineIndex - 1;
                                        }
                                    }
                                }
                                catch
                                {
                                    Debug.Error("Can not use that operator in if condition with un-float values at line " + EqualityToken.Root.LineIndex);
                                }
                            }
                            else if (EqualityToken.VALUE == "!=")
                            {
                                //Console.WriteLine($"[{LeftToken.VALUE}] ?= [{RightToken.VALUE}]");
                                if (LeftToken.VALUE != RightToken.VALUE)
                                {
                                    Debug.Success("TRUE");


                                    if (isElse)
                                    {
                                        foreach (var ElseLine in elseLines)
                                        {
                                            ElseLine.isGonnaPassed = true;
                                        }
                                    }
                                }
                                else
                                {
                                    Debug.Warning("FALSE");
                                    if (isElse)
                                    {
                                        Program.CurrentReadingLine = elseLines.First().LineIndex - 2;
                                    }
                                    else
                                    {

                                        Program.CurrentReadingLine = runLines.Last().LineIndex - 1;
                                    }
                                }
                            }


                        }
                        else
                        {
                            Debug.Error("Value on the right be found. At line " + contidionTokens[0].Root.LineIndex);
                        }

                    }
                    else
                    {
                        Debug.Error("Value on the left be found. At line " + contidionTokens[0].Root.LineIndex);

                    }





                }
                else if (tokens[0].VALUE == "BREAK")
                {
                    if (calledInLoop)
                    {
                        isBreak = true;
                    }
                    else
                    {
                        Debug.Warning("Break command could only in loops called.");
                    }
                }
                else if (tokens[0].VALUE == "EXIT")
                {
                    Environment.Exit(0);
                }
                else if (tokens[0].VALUE == "block")
                {

                    if (calledInLoop)
                    {
                        Debug.Error("You can't add a block in a loop!");
                    }
                    if (TryGetTokenAtIndex(tokens, 1, out Token name_token))
                    {
                        if (char.IsDigit(name_token.VALUE[0]))
                        {
                            Debug.Error($"A block could not start with a digit. At line {tokens[0].Root.LineIndex}");
                        }

                        var runLines = LinesBetweenType(Program.LastLines, tokens[0].Root.LineIndex, Token.TOKEN_TYPE.CON_START, Token.TOKEN_TYPE.CON_END);

                        var parameters_string = StringGetValueBetweenType(tokens, Token.TOKEN_TYPE.BREC_START, Token.TOKEN_TYPE.BREC_END);
                        parameters_string = parameters_string.Trim();
                        string[] param_list = parameters_string.Split(',');

                        if (param_list.Length == 0)
                        {
                            param_list[0] = parameters_string;
                        }

                        Debug.Info($"Parameter : {runLines[0].Priority} " + parameters_string);

                        AddOverideBlock(name_token.VALUE, param_list, runLines);

                        Program.CurrentReadingLine = runLines.Last().LineIndex - 1 + runLines.Last().Priority;

                        if(name_token.VALUE == "main")
                        {
                            //RUN BLOCK
                            for (Program.CurrentReadingLine = Blocks.Find(x => x.Name == "main").Lines.First().LineIndex - 1; Program.CurrentReadingLine < Blocks.Find(x => x.Name == "main").Lines.Last().LineIndex; Program.CurrentReadingLine++)
                            {

                                Engine.ConvertAllGlobals(Program.LastLines[Program.CurrentReadingLine].Tokens);
                                Engine.ConvertAllVariables(Program.LastLines[Program.CurrentReadingLine].Tokens);
                                Engine.ConvertAllMath(Program.LastLines[Program.CurrentReadingLine].Tokens);
                                RunTokens(Program.LastLines[Program.CurrentReadingLine], Program.LastLines[Program.CurrentReadingLine].Tokens, false, true);


                            }
                            //END BLOCK
                        }
                    }
                    else
                    {
                        Debug.Error($"Block must have a name to call. At line {tokens[0].Root.LineIndex}");
                    }
                }
                else if (tokens[0].VALUE == "out")
                {
                    if (!calledInBlock)
                    {
                        Debug.Error("'out' command could only from a block called.");
                    }
                    Engine.ConvertAllGlobals(tokens);
                    Engine.ConvertAllVariables(tokens);
                    Engine.ConvertAllMath(tokens);



                    if (TryGetTokenAtIndex(tokens, 1, out Token name_token))
                    {
                        if (Char.IsDigit(name_token.STATIC_VALUE[0]))
                        {
                            Debug.Error($"A variable could not start with a digit. At line {tokens[0].Root.LineIndex}");
                            return;
                        }
                        var to_attach_value = StringGetValueBetweenType(tokens, Token.TOKEN_TYPE.BREC_START, Token.TOKEN_TYPE.BREC_END);

                        name_token.VALUE = name_token.VALUE.Trim();
                        AddVariable(name_token.VALUE, to_attach_value);
                        Debug.Success($"[{name_token.VALUE}] outed as [{to_attach_value}]");
                    }
                    else
                    {
                        Debug.Error($"Name of the variable could not be found at line {tokens[0].Root.LineIndex}");
                    }

                }
                else if (tokens[0].VALUE == "call")
                {
                    //call x(parameters)
                    if (TryGetTokenAtIndex(tokens, 1, out Token name_token))
                    {
                        if (char.IsDigit(name_token.VALUE[0]))
                        {
                            Debug.Error($"A call method could not start with a digit. At line {tokens[0].Root.LineIndex}");
                        }

                        var parameters_string = StringGetValueBetweenType(tokens, Token.TOKEN_TYPE.BREC_START, Token.TOKEN_TYPE.BREC_END);
                        parameters_string = parameters_string.Trim();
                        
                        string[] param_list = parameters_string.Split(',');

                        /*
                        if (param_list[0] == "")
                        {
                            Debug.Error($"All of the parameters must be included for block '{name_token.VALUE}' at line {name_token.Root.LineIndex} {Environment.NewLine} Given parameters : '{parameters_string}'");
                            param_list = null;
                        }
                        */

                        Debug.Info("Send Parameter : " + parameters_string);

                        if (TryGetBlock(name_token.VALUE, out Block _block))
                        {
                            Debug.Success($"Block found : '{name_token.VALUE}'");
                        }
                        else
                        {
                            Debug.Error($"Block '{name_token.VALUE}' could not be found. At line {name_token.Root.LineIndex}");
                        }

                        //PARAMETER CHECK

                        if (_block.Paramaters.Length > 0)
                        {

                            if (_block.Paramaters.Length != param_list.Length)
                                Debug.Error($"All of the parameters must be included for block '{name_token.VALUE}' at line {name_token.Root.LineIndex} {Environment.NewLine} Given parameters : {parameters_string}");
                        }

                        int call_index = Program.CurrentReadingLine + 1;

                        for (int i = 0; i < _block.Paramaters.Length; i++)
                        {
                            _block.Paramaters[i] = _block.Paramaters[i].Trim();
                            AddVariable(_block.Paramaters[i], param_list[i]);
                        }

                        //RUN BLOCK
                        for (Program.CurrentReadingLine = _block.Lines.First().LineIndex - 1; Program.CurrentReadingLine < _block.Lines.Last().LineIndex; Program.CurrentReadingLine++)
                        {

                            Engine.ConvertAllGlobals(Program.LastLines[Program.CurrentReadingLine].Tokens);
                            Engine.ConvertAllVariables(Program.LastLines[Program.CurrentReadingLine].Tokens);
                            Engine.ConvertAllMath(Program.LastLines[Program.CurrentReadingLine].Tokens);
                            RunTokens(Program.LastLines[Program.CurrentReadingLine], Program.LastLines[Program.CurrentReadingLine].Tokens, false, true);


                        }
                        //END BLOCK

                        for (int i = 0; i < _block.Paramaters.Length; i++)
                        {
                            _block.Paramaters[i] = _block.Paramaters[i].Trim();

                            if (!_block.Paramaters[i].Contains("&"))
                            {
                                RemoveVariable(_block.Paramaters[i]);
                            }


                        }

                        //BACK TO CALL LINE
                        Debug.Success("Called from : " + call_index);
                        Program.CurrentReadingLine = call_index;
                    }
                    else
                    {
                        Debug.Error($"Block must have a name to call. At line {tokens[0].Root.LineIndex}");
                    }
                }
                else if (tokens[0].VALUE == "WINDOW")
                {
                    if (TryGetTokenAtIndex(tokens, 1, out Token OperationToken))
                    {
                        Console.WriteLine($"[{OperationToken.VALUE}]");
                        if (OperationToken.VALUE == "CREATE")
                        {

                            var WindowsParam = StringGetValueBetweenType(tokens, Token.TOKEN_TYPE.BREC_START, Token.TOKEN_TYPE.BREC_END);
                            if (WindowsParam == null)
                            {
                                Debug.Error($"Please enter the start parameters at line '{RunningLine.LineIndex}'");
                            }

                            string[] param = WindowsParam.Split(',');

                            if (param.Length < 4)
                            {
                                Debug.Error($"Please include all argument to run form at line {RunningLine.LineIndex}");
                            }


                          
                            Form.CheckForIllegalCrossThreadCalls = false;
                            Form newForm = new Form();

                            newForm.Text = param[0];
                            newForm.Size = new System.Drawing.Size(int.Parse(param[1]), int.Parse(param[2]));
                            newForm.ShowIcon = false;
                            newForm.SizeGripStyle = SizeGripStyle.Hide;
                            newForm.FormBorderStyle = FormBorderStyle.FixedSingle;
                            
                            

                            Debug.Success($"'{WindowsParam}' is being shown...");
                            Windows.Add(new Window(newForm, param[0], int.Parse(param[1]), int.Parse(param[2]), param[3].Trim()));
                      
                        }
                        else if(OperationToken.VALUE == "RENDER")
                        {
                            var ACode = StringGetValueBetweenType(tokens, Token.TOKEN_TYPE.BREC_START, Token.TOKEN_TYPE.BREC_END);

                            Window SearchedWindow = Windows.Find(x => x.ACCESSNAME == ACode);

                            if (SearchedWindow == null)
                            {
                                Debug.Error($"Could not find that window with name '{ACode}' at line {RunningLine.LineIndex}");
                            }

                            Task.Run(() =>
                            {
                                SearchedWindow.CurrentForm.Show();
                                Application.Run(SearchedWindow.CurrentForm);
                            });
                          

                        }
                        else if (OperationToken.VALUE == "CLOSE")
                        {
                            var ACode = StringGetValueBetweenType(tokens, Token.TOKEN_TYPE.BREC_START, Token.TOKEN_TYPE.BREC_END);

                            Window SearchedWindow = Windows.Find(x => x.ACCESSNAME == ACode);

                            if (SearchedWindow == null)
                            {
                                Debug.Error($"Could not find that window with name '{ACode}' at line {RunningLine.LineIndex}");
                            }

                            SearchedWindow.CurrentForm.Close();
                        }
                        else if (OperationToken.VALUE == "COMPONENT")
                        {
                     
                            if (TryGetTokenAtIndex(tokens, 2, out Token COMPONENTDOTOKEN))
                            {
                                if(COMPONENTDOTOKEN.VALUE == "ADD")
                                {
                                    var Arguments = StringGetValueBetweenType(tokens, Token.TOKEN_TYPE.BREC_START, Token.TOKEN_TYPE.BREC_END);

                                    string[] Parsed = Arguments.Split(',');
                                    Parsed[0] = Parsed[0].Trim();

                                    int i = 0;
                                    foreach(var parseValue in Parsed)
                                    {
                                        Parsed[i] = parseValue.Trim();
                                        i++;
                                    }

                                    if (Parsed.Length < 2)
                                    {
                                        Debug.Error($"Please include all argument to run form at line {RunningLine.LineIndex}");
                                    }


                                    Window SearchedWindow = Windows.Find(x => x.ACCESSNAME == Parsed[0]);

                                    if (SearchedWindow == null)
                                    {
                                        Debug.Error($"Could not find that window with name '{Parsed[0]}' at line {RunningLine.LineIndex}");
                                    }


                                    if(Parsed[1] == "TEXT")
                                    {
                                        Label toAddLabel = new Label();
                                        toAddLabel.Text = Parsed[2];
                                        toAddLabel.Dock = DockStyle.Top;
                                        SearchedWindow.CurrentForm.Controls.Add(toAddLabel);
                                    }
                               
                                 
                                }
                                else
                                {
                                    Debug.Error($"Unknown operation : '{OperationToken.VALUE}' at line '{RunningLine.LineIndex}'");
                                }

                            }
                        }
                        else
                        {
                            Debug.Error($"Unknown operation : '{OperationToken.VALUE}' at line '{RunningLine.LineIndex}'");
                        }


                    }
                    else
                    {
                        Debug.Error($"Operation could not be found at line {RunningLine.LineIndex}");
                    }
                }
                else if (tokens[0].VALUE == "array")
                {
                    if (TryGetTokenAtIndex(tokens, 1, out Token OperationToken))
                    {
                        if (OperationToken.VALUE == "create")
                        {

                            var ToName = StringGetValueBetweenType(tokens, Token.TOKEN_TYPE.BREC_START, Token.TOKEN_TYPE.BREC_END);
                            if (ToName == null)
                            {
                                Debug.Error($"Please enter the name of the array at line '{RunningLine.LineIndex}'");
                            }

                            if (Arrays.Find(x => x.Name == ToName) != null)
                            {
                                Debug.Error($"There is already a array named '{ToName}' at line '{RunningLine.LineIndex}'");
                            }

                            Arrays.Add(new Karray(ToName));
                            Debug.Success($"'{ToName}' has been added!");
                        }
                        else if (OperationToken.VALUE == "add")
                        {
                            // array add x 

                            if (TryGetTokenAtIndex(tokens, 2, out Token ArrayNameToken))
                            {
                                Karray thisArray = Arrays.Find(x => x.Name == ArrayNameToken.VALUE);

                                if (thisArray == null)
                                {
                                    Debug.Error($"Given array could not be found at line '{RunningLine.LineIndex}'.Use 'array create({ArrayNameToken.VALUE})'");
                                }

                                var AddValue = StringGetValueBetweenType(tokens, Token.TOKEN_TYPE.BREC_START, Token.TOKEN_TYPE.BREC_END);

                                if (AddValue == null)
                                {
                                    Debug.Error($"Please enter the value to add array at line '{RunningLine.LineIndex}'");
                                }

                                thisArray.Content.Add(AddValue);



                            }
                            else
                            {
                                Debug.Error($"Name of the array is missing at line '{RunningLine.LineIndex}'");
                            }

                        }
                        else
                        {
                            Debug.Error($"Unknown operation : '{OperationToken.VALUE}' at line '{RunningLine.LineIndex}'");
                        }


                    }
                    else
                    {
                        Debug.Error($"Operation could not be found at line {RunningLine.LineIndex}");
                    }
                }
                else
                {
                    if (tokens[0].VALUE != "")
                    {
                        if (tokens[0].TYPE != Token.TOKEN_TYPE.CON_START && tokens[0].TYPE != Token.TOKEN_TYPE.CON_END && tokens[0].TYPE != Token.TOKEN_TYPE.GLOBAL && tokens[0].TYPE != Token.TOKEN_TYPE.ELSE && tokens[0].TYPE != Token.TOKEN_TYPE.COMMENT)
                        {
                            Debug.Error($"Unknown Command : '{tokens[0].VALUE}' at line {tokens[0].Root.LineIndex}");

                        }

                    }

                }
            }

            if (tokens.Count > 0)
            {
                Thread.Sleep(Program.LINE_RUN_DELAY);

            }




        }

        private static List<Line> LinesBetweenType(List<Line> lines, int start_index,Token.TOKEN_TYPE type1, Token.TOKEN_TYPE type2)
        {
            try
            {
                Line Start = lines.Find(x => x.Tokens.Count > 0 && x.Tokens[0].TYPE == type1 && x.LineIndex > start_index);

                Line End = lines.Find(x => x.Tokens.Count > 0 && x.Tokens[0].TYPE == type2 && x.LineIndex > Start.LineIndex && x.Tokens[0].LAYER == Start.Tokens[0].LAYER);


                var between_lines = lines.FindAll(x => x.LineIndex > Start.LineIndex && x.LineIndex < End.LineIndex);

                return between_lines;
            }
            catch
            {
                return null;
            }

   
        }

        public static void NormalizeLines(List<Line> Lines)
        {
            int i = 1;
            foreach(Line line in Lines)
            {
                
                line.LineIndex = i;
                i++;
                
               
            }

            

        }


        private static string StringGetValueBetweenType(List<Token> tokens, Token.TOKEN_TYPE type1, Token.TOKEN_TYPE type2, bool returnStatics = false, int union = 0, bool isEcho = false)
        {
            
            string TO_RETURN = "";

            try
            {

                int k = 0;
                int lastIndex = 0;
                Token first = null;
                while(true)
                {
                   
                    first = tokens.Find(x => x.TYPE == type1 && x.INDEX > lastIndex);

                    if (first == null)
                    {
                        if(k == 0)
                        {
                            break;
                        }
                        else
                        {
                            Debug.Error($"Could not find with type {type1} at line {tokens[0].Root.LineIndex}. Union : {union}");
                        }
                       
                        break;
                    }
                    if (k >= union)
                    {
                        Debug.Success(first.INDEX.ToString());
                        break;
                    }
                    
                  
                    lastIndex = first.INDEX;

                    k++;

                }

                k = 0;
                lastIndex = tokens.Count;
                Token last = null;
                while (true)
                {


                    last = tokens.FindLast(x => x.TYPE == type2 && x.INDEX > first.INDEX && x.INDEX < lastIndex);
                    if (last == null)
                    {
                        if(k == 0)
                        {
                            break;
                        }
                        else
                        {
                            Debug.Error($"Could not find with type {type2} at line {tokens[0].Root.LineIndex}. Union : {union}");
                        }
                        
                        break;
                    }
                    lastIndex = last.INDEX;

                    if (k >= union)
                    {
                        Debug.Success(last.INDEX.ToString());
                        break;
                    }

                    k++;
                }

           
                

                if (first != null && last != null)
                {
                    for (int i = first.INDEX + 1; i < last.INDEX; i++)
                    {
                        if (!tokens[i].isVisible)
                        {
                            if(isEcho)
                            {
                                i++;
                                continue;
                            }
                            
                        }
                        if (returnStatics == false)
                        {
                            if (TO_RETURN == "")
                            {

                                TO_RETURN = tokens[i].VALUE.Trim();


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
    
        private static List<Token> TokensGetBetweenType(List<Token> tokens, Token.TOKEN_TYPE type1, Token.TOKEN_TYPE type2,int union = 0)
        {
            List<Token> TO_RETURN = new List<Token>();

            int k = 0;
            int lastIndex = 0;
            Token first = null;
            while (true)
            {

                first = tokens.Find(x => x.TYPE == type1 && x.INDEX > lastIndex);

                if (first == null)
                {
                    if (k == 0)
                    {
                        break;
                    }
                    else
                    {
                        Debug.Error($"Could not find with type {type1} at line {tokens[0].Root.LineIndex}. Union : {union}");
                    }

                    break;
                }
                if (k >= union)
                {
                    Debug.Success(first.INDEX.ToString());
                    break;
                }


                lastIndex = first.INDEX;

                k++;

            }

            k = 0;
            lastIndex = tokens.Count;
            Token last = null;
            while (true)
            {


                last = tokens.FindLast(x => x.TYPE == type2 && x.INDEX > first.INDEX && x.INDEX < lastIndex);
                if (last == null)
                {
                    if (k == 0)
                    {
                        break;
                    }
                    else
                    {
                        Debug.Error($"Could not find with type {type2} at line {tokens[0].Root.LineIndex}. Union : {union}");
                    }

                    break;
                }
                lastIndex = last.INDEX;

                if (k >= union)
                {
                    Debug.Success(last.INDEX.ToString());
                    break;
                }

                k++;
            }



            try
            {

                if (first != null && last != null)
                {
                    for (int i = first.INDEX + 1; i < last.INDEX; i++)
                    {
                        TO_RETURN.Add(tokens[i]);


                    }
                }
                else
                {
                    return null;
                }


            }
            catch (Exception ex)
            {
                Debug.Error(ex.Message);
                if (ex == default(ArgumentNullException))
                {
                    Debug.Error($"At line {tokens[0].Root.LineIndex} could not find the type : {type1} or {type2}");
                }

                return null;

            }

            return TO_RETURN;


        }
    }
}
