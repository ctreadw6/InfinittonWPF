using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;


namespace InfinittonWPF
{
    public class TextStringAction  : IButtonPressAction
    {
        public String Value { get; set; }
        InputSimulator sim = new InputSimulator();

        public override void DoStuff(MainController controller, int buttonNum)
        {
            if (Value == null) return;
            sim.Keyboard.TextEntry(Value);

        }

        public override String GetDefaultIconPath()
        {
            return "textStringIcon.png";
        }

    }
}
