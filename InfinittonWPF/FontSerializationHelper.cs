using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WpfColorFontDialog;
using FontStyle = System.Drawing.FontStyle;

namespace InfinittonWPF
{
    internal class FontSerializationHelper
    {
        public static FontInfo Deserialize(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            var arr = value.Split(new[] {','});
            if (arr.Length < 3) return null;
            return new FontInfo(new FontFamily(arr[0]), double.Parse(arr[1]), FontStyles.Normal, FontStretches.Normal, FontWeights.Normal, new SolidColorBrush((Color)ColorConverter.ConvertFromString(arr[2])));
        }

        public static string Serialize(FontInfo value)
        {
            string str;
            var c = value.Color.Brush.Color;
            str = value.Family + "," + value.Size.ToString() + "," + "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
            return str;
        }
    }
}
