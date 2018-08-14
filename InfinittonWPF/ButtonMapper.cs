using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfinittonWPF
{
    public class ButtonMapper
    {
        public static List<UInt16> Buttons = new List<UInt16>()
        {
            0x0100,
            0x0200,
            0x0400,
            0x0800,
            0x1000,
            0x2000,
            0x4000,
            0x8000,
            0x0001,
            0x0002,
            0x0004,
            0x0008,
            0x0010,
            0x0020,
            0x0040
        };

        public static int GetButtonIndex(UInt16 val)
        {
            if (Buttons.Contains(val)) return Buttons.IndexOf(val);
            return -1;
        }
    }
}
