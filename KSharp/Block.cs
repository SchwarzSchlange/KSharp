using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSharp
{
    public class Block
    {
        public string Name { get; set; }
        public string[] Paramaters { get; set; }
        public List<Line> Lines { get; set; }

        public Block(string _Name,List<Line> _Lines,string[] _Paramaters)
        {
            this.Name = _Name;
            this.Lines = _Lines;
            this.Paramaters = _Paramaters;
        }

    }
}
