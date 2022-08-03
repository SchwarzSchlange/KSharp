using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSharp
{
    public class Token
    {
        public enum TOKEN_TYPE
        {
            COMMAND,
            NORMAL,

            BREC_START,
            BREC_END,
            QUOTE,
            CON_START,
            CON_END,
            VARIABLE,
            GLOBAL,
            NULL,
            PLUS,
            MINUS,
            DIVIDER,
            MULTIP,
            MATH_START,
            MATH_END,
            MATH_TO_UPDATE,
            COMMA,
            CONDITION
        };

        public int INDEX { get; set; }
        public TOKEN_TYPE TYPE { get; set; }
        public string VALUE { get; set; }
        public string STATIC_VALUE { get; set; }
        public int LAYER { get; set; }
        public int UNION { get; set; }
        public Line Root { get; set; }
        

        public Token(int index,TOKEN_TYPE type,string value,Line root,int layer = 0,int union = 0)
        {
            this.INDEX = index;
            this.TYPE = type;
            this.VALUE = value;
            this.Root = root;
            STATIC_VALUE = value;
            this.LAYER = layer;
            this.UNION = union;
        }
  
    }
}
