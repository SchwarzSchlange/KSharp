using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSharp
{
    class Karray
    {

        public string Name { get; set; }

        public List<object> Content { get; set;}


        public Karray(string _Name)
        {

            this.Name = _Name;
            this.Content = new List<object>();
        }
    }
}
