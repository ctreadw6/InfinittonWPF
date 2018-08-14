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
    public class MacroAction  : IButtonPressAction
    {
        public List<Object> KeyObjects = new List<object>();
        InputSimulator sim = new InputSimulator();

        public override void DoStuff(MainController controller, int buttonNum)
        {
            if (KeyObjects == null || KeyObjects.Count <= 0) return;
            foreach (var obj in KeyObjects)
            {
                if (obj is SleepObject)
                {
                    var sleepObj = obj as SleepObject;
                    sim.Keyboard.Sleep(sleepObj.sleepTime);
                }
                else if (obj is KeyObject)
                {
                    var keyObj = obj as KeyObject;
                    if (keyObj.ModifierSet && keyObj.KeySet)
                    {
                        sim.Keyboard.ModifiedKeyStroke(keyObj.Modifier, keyObj.KeyCode);
                    }
                    else if (keyObj.KeySet)
                    {
                        sim.Keyboard.KeyPress(keyObj.KeyCode);
                    }

                }
            }
        }

        public override String GetDefaultIconPath()
        {
            return "textStringIcon.png";
        }

        public class SleepObject
        {
            public int sleepTime { get; set; }
        }

        public class KeyObject
        {
            public bool ModifierSet = false;
            public bool KeySet = false;

            private VirtualKeyCode _KeyCode;

            public VirtualKeyCode KeyCode
            {
                get { return _KeyCode; }
                set
                {
                    _KeyCode = value;
                    KeySet = true;
                }
            }

            private VirtualKeyCode _Modifier;

            public VirtualKeyCode Modifier
            {
                get { return _Modifier; }
                set
                {
                    _Modifier = value;
                    ModifierSet = true;
                }
            }
        }
    }
}
