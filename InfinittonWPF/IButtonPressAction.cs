using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Drawing.Text;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfColorFontDialog;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Drawing.Color;
using FontFamily = System.Windows.Media.FontFamily;

namespace InfinittonWPF
{
    public abstract class IButtonPressAction : INotifyPropertyChanged
    {
        public virtual String GetDefaultIconPath()
        {
            return null;
        }
        public String Title { get; set; } = "";

        //private String _IconPath = null;
        //public String IconPath
        //{
        //    get { return _IconPath ?? GetDefaultIconPath(); }
        //    set { _IconPath = value; OnPropertyChanged("IconPath"); }
        //}

        public Color BackgroundColor { get; set; } = Color.Black;
        public FontInfo Titlefont { get; set; } = new FontInfo() {Size = 9, BrushColor = Brushes.White, Family = new FontFamily()};
        public bool ShowTitleLabel { get; set; } = false;

        public Image Icon { get; set; } = null;

        public BitmapSource GetBitmapSource
        {
            get
            {
                using (var ms = new MemoryStream())
                {
                    GetIcon.Save(ms, ImageFormat.Bmp);
                    ms.Seek(0, SeekOrigin.Begin);

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = ms;
                    bitmapImage.EndInit();

                    return bitmapImage;
                }
            }
        }

        public Image GetIcon
        {
            get { return PictureConverter.Superimpose((Bitmap)Icon, BackgroundColor); }
            set { Icon = value; }
        }

        public abstract void DoStuff(MainController controller, int buttonNum);
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
