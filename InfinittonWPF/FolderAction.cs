using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfinittonWPF
{

    public class FolderAction : IButtonPressAction
    {
        public override String GetDefaultIconPath()
        {
            return "Folder-Icon.png";
        }

        public override void DoStuff(MainController controller, int buttonNum)
        {
            controller.ChangeFolder(buttonNum);
        }

        public String ExeConditionName { get; set; }
    }

    public class HomeFolderAction : FolderAction
    {
        public override String GetDefaultIconPath()
        {
            return "home.png";
        }

        public override void DoStuff(MainController controller, int buttonNum)
        {
            controller.ChangeFolder(-1);
        }
    }
}
