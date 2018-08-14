using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfinittonWPF
{
    public class LaunchAction : IButtonPressAction
    {
        public String ExePath { get; set; }
        public String Args { get; set; }

        public override void DoStuff(MainController controller, int buttonNum)
        {
            if (ExePath == null) return;
            if (Args == null) Args = "";
            Process.Start(new ProcessStartInfo(ExePath, Args));
        }

        public override String GetDefaultIconPath()
        {
            return "GoExe.PNG";
        }

    }
}
