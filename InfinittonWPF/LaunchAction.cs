using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfinittonWPF
{
    public class LaunchAction : IButtonPressAction
    {
        public String ExePath { get; set; } = "";
        public String Args { get; set; } = "";
        public ProcessRunningAction AlreadyRunningAction { get; set; }

        public override void DoStuff(MainController controller, int buttonNum)
        {
            try
            {
                if (ExePath == null) return;
                ExePath = ExePath.Trim(new[] {'"'});
                if (Args == null) Args = "";
                switch (AlreadyRunningAction)
                {
                    case ProcessRunningAction.NewProcess:
                        Process.Start(new ProcessStartInfo(ExePath, Args));
                        break;
                    case ProcessRunningAction.FocusOldProcess:
                    case ProcessRunningAction.KillOldProcess:
                        var oldProcess = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ExePath))
                            .FirstOrDefault();
                        if (oldProcess == null) Process.Start(new ProcessStartInfo(ExePath, Args));
                        else if (AlreadyRunningAction == ProcessRunningAction.FocusOldProcess)
                        {
                            MainController.ShowWindow(oldProcess.MainWindowHandle);
                            MainController.SetForegroundWindow(oldProcess.MainWindowHandle);
                        }
                        else
                        {
                            oldProcess.Kill();
                        }

                        break;
                }
            }
            catch (Exception exc)
            {

            }

        }

        public override String GetDefaultIconPath()
        {
            return "GoExe.PNG";
        }


        public enum ProcessRunningAction
        {
            FocusOldProcess = 0,
            NewProcess,
            KillOldProcess
        }

    }
}
