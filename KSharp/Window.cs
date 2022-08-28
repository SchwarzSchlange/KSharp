using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace KSharp
{
    class Window
    {
        public Form CurrentForm { get; private set; }
        public string TITLE { get; set; }
        public int WIDTH { get; set; }
        public int HEIGHT { get; set; }
        public string ACCESSNAME { get; private set; }
     


        public Window(Form _currentform,string t,int w,int h,string aname)
        {
            this.CurrentForm = _currentform;
            this.TITLE = t;
            this.WIDTH = w;
            this.HEIGHT = h;
            this.ACCESSNAME = aname;
        }
    }
}
