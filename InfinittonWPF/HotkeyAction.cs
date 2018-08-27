using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;


namespace InfinittonWPF
{
    public class HotkeyAction  : IButtonPressAction
    {
        public List<VirtualKeyCode> Modifiers = new List<VirtualKeyCode>();
        public VirtualKeyCode MainKey = (VirtualKeyCode)0;
        InputSimulator sim = new InputSimulator();

        public override void DoStuff(MainController controller, int buttonNum)
        {
            sim.Keyboard.ModifiedKeyStroke(Modifiers, MainKey);
        }

        public override String GetDefaultIconPath()
        {
            return "hotkeyIcon.png";
        }

        public override string ToString()
        {
            return Utils.GetKeysString(Modifiers, MainKey);
        }
    }
}
