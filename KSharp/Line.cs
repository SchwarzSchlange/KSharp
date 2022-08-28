using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSharp
{
    public class Line
    {
        public int LineIndex { get; set; }
        public List<Token> Tokens { get; set; }
        public bool isGonnaPassed = false;
        public bool isIncluded = false;
        public int Priority = 0;

    }
}
