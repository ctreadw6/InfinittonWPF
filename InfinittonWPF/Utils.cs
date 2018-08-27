using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using WindowsInput.Native;

namespace InfinittonWPF
{
    public static class Utils
    {
        public static Bitmap GetBitmap(BitmapSource source)
        {
            if (source == null) return null;
            Bitmap bmp = null;

            source.Dispatcher.Invoke((Action)delegate
            {
                bmp = new Bitmap
                (
                  source.PixelWidth,
                  source.PixelHeight,
                  System.Drawing.Imaging.PixelFormat.Format32bppPArgb
                );


                BitmapData data = bmp.LockBits
                (
                    new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
                    ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppPArgb
                );

                source.CopyPixels
                (
                  Int32Rect.Empty,
                  data.Scan0,
                  data.Height * data.Stride,
                  data.Stride
                );

                bmp.UnlockBits(data);
            });

            return bmp;
        }

        public static BitmapSource GetBitmapSource(Bitmap bitmap)
        {
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap
            (
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );

            return bitmapSource;
        }

        public static string GetKeysString(List<VirtualKeyCode> Modifiers, VirtualKeyCode MainKey)
        {
            List<String> ret = new List<string>();
            foreach (var mod in Modifiers)
            {
                ret.Add(GetKeyString(mod));
            }

            ret.Add(GetKeyString(MainKey));

            return string.Join(" + ", ret);
        }

        public static String GetKeyString(VirtualKeyCode key)
        {
            int KeyValue = (int)key;
            String keyString = "";
            switch (KeyValue)
            {
                case 0:
                    break;
                case 1:
                    keyString = "Mouse Left";
                    break;
                case 2:
                    keyString = "Mouse Right";
                    break;
                case 3:
                    keyString = "Cancel";
                    break;
                case 4:
                    keyString = "Mouse Middle";
                    break;
                case 5:
                    keyString = "Special 1";
                    break;
                case 6:
                    keyString = "Special 2";
                    break;
                case 8:
                    keyString = "Back";
                    break;
                case 9:
                    keyString = "TAB";
                    break;
                case 12:
                    keyString = "Clear";
                    break;
                case 13:
                    keyString = "Enter";
                    break;
                case 16:
                    keyString = "Shift";
                    break;
                case 17:
                    keyString = "Ctrl";
                    break;
                case 18:
                    keyString = "Alt";
                    break;
                case 19:
                    keyString = "Pause";
                    break;
                case 20:
                    keyString = "Caps Lock";
                    break;
                case 21:
                    keyString = "Kana/Hangul";
                    break;
                case 23:
                    keyString = "Junja";
                    break;
                case 24:
                    keyString = "Final";
                    break;
                case 25:
                    keyString = "Hanja/Kanji";
                    break;
                case 27:
                    keyString = "Esc";
                    break;
                case 28:
                    keyString = "Convert";
                    break;
                case 29:
                    keyString = "NonConvert";
                    break;
                case 30:
                    keyString = "Accept";
                    break;
                case 31:
                    keyString = "Mode";
                    break;
                case 32:
                    keyString = "Space";
                    break;
                case 33:
                    keyString = "Page Up";
                    break;
                case 34:
                    keyString = "Page Down";
                    break;
                case 35:
                    keyString = "End";
                    break;
                case 36:
                    keyString = "Home";
                    break;
                case 37:
                    keyString = "Left";
                    break;
                case 38:
                    keyString = "Up";
                    break;
                case 39:
                    keyString = "Right";
                    break;
                case 40:
                    keyString = "Down";
                    break;
                case 41:
                    keyString = "Select";
                    break;
                case 42:
                    keyString = "Print";
                    break;
                case 43:
                    keyString = "Execute";
                    break;
                case 44:
                    keyString = "Snapshot";
                    break;
                case 45:
                    keyString = "Insert";
                    break;
                case 46:
                    keyString = "Delete";
                    break;
                case 47:
                    keyString = "Help";
                    break;
                case 48:
                    keyString = "Num 0";
                    break;
                case 49:
                    keyString = "Num 1";
                    break;
                case 50:
                    keyString = "Num 2";
                    break;
                case 51:
                    keyString = "Num 3";
                    break;
                case 52:
                    keyString = "Num 4";
                    break;
                case 53:
                    keyString = "Num 5";
                    break;
                case 54:
                    keyString = "Num 6";
                    break;
                case 55:
                    keyString = "Num 7";
                    break;
                case 56:
                    keyString = "Num 8";
                    break;
                case 57:
                    keyString = "Num 9";
                    break;
                case 65:
                    keyString = "A";
                    break;
                case 66:
                    keyString = "B";
                    break;
                case 67:
                    keyString = "C";
                    break;
                case 68:
                    keyString = "D";
                    break;
                case 69:
                    keyString = "E";
                    break;
                case 70:
                    keyString = "F";
                    break;
                case 71:
                    keyString = "G";
                    break;
                case 72:
                    keyString = "H";
                    break;
                case 73:
                    keyString = "I";
                    break;
                case 74:
                    keyString = "J";
                    break;
                case 75:
                    keyString = "K";
                    break;
                case 76:
                    keyString = "L";
                    break;
                case 77:
                    keyString = "M";
                    break;
                case 78:
                    keyString = "N";
                    break;
                case 79:
                    keyString = "O";
                    break;
                case 80:
                    keyString = "P";
                    break;
                case 81:
                    keyString = "Q";
                    break;
                case 82:
                    keyString = "R";
                    break;
                case 83:
                    keyString = "S";
                    break;
                case 84:
                    keyString = "T";
                    break;
                case 85:
                    keyString = "U";
                    break;
                case 86:
                    keyString = "V";
                    break;
                case 87:
                    keyString = "W";
                    break;
                case 88:
                    keyString = "X";
                    break;
                case 89:
                    keyString = "Y";
                    break;
                case 90:
                    keyString = "Z";
                    break;
                case 91:
                    keyString = "Windows Left";
                    break;
                case 92:
                    keyString = "Windows Right";
                    break;
                case 93:
                    keyString = "Application";
                    break;
                case 95:
                    keyString = "Sleep";
                    break;
                case 96:
                    keyString = "NumPad 0";
                    break;
                case 97:
                    keyString = "NumPad 1";
                    break;
                case 98:
                    keyString = "NumPad 2";
                    break;
                case 99:
                    keyString = "NumPad 3";
                    break;
                case 100:
                    keyString = "NumPad 4";
                    break;
                case 101:
                    keyString = "NumPad 5";
                    break;
                case 102:
                    keyString = "NumPad 6";
                    break;
                case 103:
                    keyString = "NumPad 7";
                    break;
                case 104:
                    keyString = "NumPad 8";
                    break;
                case 105:
                    keyString = "NumPad 9";
                    break;
                case 106:
                    keyString = "NumPad *";
                    break;
                case 107:
                    keyString = "NumPad +";
                    break;
                case 108:
                    keyString = "NumPad .";
                    break;
                case 109:
                    keyString = "NumPad -";
                    break;
                case 110:
                    keyString = "NumPad ,";
                    break;
                case 111:
                    keyString = "NumPad /";
                    break;
                case 112:
                    keyString = "F1";
                    break;
                case 113:
                    keyString = "F2";
                    break;
                case 114:
                    keyString = "F3";
                    break;
                case 115:
                    keyString = "F4";
                    break;
                case 116:
                    keyString = "F5";
                    break;
                case 117:
                    keyString = "F6";
                    break;
                case 118:
                    keyString = "F7";
                    break;
                case 119:
                    keyString = "F8";
                    break;
                case 120:
                    keyString = "F9";
                    break;
                case 121:
                    keyString = "F10";
                    break;
                case 122:
                    keyString = "F11";
                    break;
                case 123:
                    keyString = "F12";
                    break;
                case 124:
                    keyString = "F13";
                    break;
                case 125:
                    keyString = "F14";
                    break;
                case 126:
                    keyString = "F15";
                    break;
                case 127:
                    keyString = "F16";
                    break;
                case 128:
                    keyString = "F17";
                    break;
                case 129:
                    keyString = "F18";
                    break;
                case 130:
                    keyString = "F19";
                    break;
                case 131:
                    keyString = "F20";
                    break;
                case 132:
                    keyString = "F21";
                    break;
                case 133:
                    keyString = "F22";
                    break;
                case 134:
                    keyString = "F23";
                    break;
                case 135:
                    keyString = "F24";
                    break;
                case 144:
                    keyString = "Num lock";
                    break;
                case 145:
                    keyString = "Scroll";
                    break;
                case 160:
                    keyString = "Shift Left";
                    break;
                case 161:
                    keyString = "Shift Right";
                    break;
                case 162:
                    keyString = "Ctrl Left";
                    break;
                case 163:
                    keyString = "Ctrl Right";
                    break;
                case 164:
                    keyString = "Menu Left";
                    break;
                case 165:
                    keyString = "Menu Right";
                    break;
                default:
                    break;
            }
            return keyString;
        }
    }
}
