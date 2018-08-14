using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfinittonWPF
{
    class NullAction : IButtonPressAction
    {
        public override void DoStuff(MainController controller, int buttonNum)
        {
            return;
        }
    }
}
