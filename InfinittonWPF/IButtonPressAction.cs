using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace InfinittonWPF
{
    public abstract class IButtonPressAction : INotifyPropertyChanged
    {
        public virtual String GetDefaultIconPath()
        {
            return "Grey.PNG";
        }
        public String Title { get; set; } = "";

        private String _IconPath = null;
        public String IconPath
        {
            get { return _IconPath ?? GetDefaultIconPath(); }
            set { _IconPath = value; OnPropertyChanged("IconPath"); }
        }

        public Image Icon
        {
            get { return Bitmap.FromFile(IconPath); }
        }

        public abstract void DoStuff(MainController controller, int buttonNum);
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
