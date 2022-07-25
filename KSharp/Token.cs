using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSharp
{
    class Token
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
            NULL

           
        };

        public int INDEX { get; set; }
        public TOKEN_TYPE TYPE { get; set; }
        public string VALUE { get; set; }
        public Line Root { get; set; }


  
    }
}
