using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TsudaKageyu;
using Application = System.Windows.Application;
using Brushes = System.Windows.Media.Brushes;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using Image = System.Windows.Controls.Image;

namespace InfinittonWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public String StartProgramActionPath
        {
            get { return "GoExe.png"; }
        }

        public String FolderActionPath
        {
            get { return "Folder-icon.png"; }
        }

        public String TextStringActionPath
        {
            get { return "textStringIcon.png"; }
        }

        public int SelectedNumber = -1;
        public Image SelectedImage = null;

        private ObservableCollection<IButtonPressAction> _Actions = new ObservableCollection<IButtonPressAction>();

        public event PropertyChangedEventHandler PropertyChanged;

        public void Notify(String val)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(val));
            }
        }

        public ObservableCollection<IButtonPressAction> Actions
        {
            get { return _Actions; }
        }

        // This is terrible, but it lets me bind it to the UI easily. 
        // Probably should refactor this it some point.
        public String Action0
        {
            get { return Actions[0].IconPath; }
            set { Actions[0].IconPath = value; Notify("Action0");}
        }

        public String Action1
        {
            get { return Actions[1].IconPath; }
            set { Actions[1].IconPath = value; Notify("Action1"); }
        }

        public String Action2
        {
            get { return Actions[2].IconPath; }
            set { Actions[2].IconPath = value; Notify("Action2"); }
        }

        public String Action3
        {
            get { return Actions[3].IconPath; }
            set { Actions[3].IconPath = value; Notify("Action3"); }
        }

        public String Action4
        {
            get { return Actions[4].IconPath; }
            set { Actions[4].IconPath = value; Notify("Action4"); }
        }

        public String Action5
        {
            get { return Actions[5].IconPath; }
            set { Actions[5].IconPath = value; Notify("Action5"); }
        }

        public String Action6
        {
            get { return Actions[6].IconPath; }
            set { Actions[6].IconPath = value; Notify("Action6"); }
        }

        public String Action7
        {
            get { return Actions[7].IconPath; }
            set { Actions[7].IconPath = value; Notify("Action7"); }
        }

        public String Action8
        {
            get { return Actions[8].IconPath; }
            set { Actions[8].IconPath = value; Notify("Action8"); }
        }

        public String Action9
        {
            get { return Actions[9].IconPath; }
            set { Actions[9].IconPath = value; Notify("Action9"); }
        }

        public String Action10
        {
            get { return Actions[10].IconPath; }
            set { Actions[10].IconPath = value; Notify("Action10"); }
        }

        public String Action11
        {
            get { return Actions[11].IconPath; }
            set { Actions[11].IconPath = value; Notify("Action11"); }
        }

        public String Action12
        {
            get { return Actions[12].IconPath; }
            set { Actions[12].IconPath = value; Notify("Action12"); }
        }

        public String Action13
        {
            get { return Actions[13].IconPath; }
            set { Actions[13].IconPath = value; Notify("Action13"); }
        }

        public String Action14
        {
            get { return Actions[14].IconPath; }
            set { Actions[14].IconPath = value; Notify("Action14"); }
        }

        private MainController controller;
        private System.Windows.Forms.NotifyIcon notifyIcon = null;

        public MainWindow()
        {
            for (int i = 0; i < 15; i++)
            {
                _Actions.Add(new NullAction());
            }
            InitializeComponent();
            this.DataContext = this;
            
        }

        private void actionImageMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image image = e.Source as Image;
            DragDrop.DoDragDrop(image, image.Tag, DragDropEffects.All);
        }

        private void imageDrop(object sender, DragEventArgs e)
        {
            Image imageControl = (Image)sender;
            int imageNum = int.Parse(imageControl.Tag as String);
            if ((e.Data.GetData(typeof(String)) != null))
            {
                String tag = e.Data.GetData(typeof(String)) as String;
                switch (tag)
                {
                    case "FolderAction":
                        Actions[imageNum] = new FolderAction();
                        break;
                    case "StartProgramAction":
                        Actions[imageNum] = new LaunchAction();
                        break;
                    case "TextStringAction":
                        Actions[imageNum] = new TextStringAction();
                        break;
                }
                controller.AddAction(imageNum + 1, Actions[imageNum]);
                Notify("Action" + imageNum.ToString());
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (File.Exists(files[0]))
                {
                    Actions[imageNum].IconPath = files[0];
                    controller.AddAction(imageNum + 1, Actions[imageNum]);
                    Notify("Action" + imageNum.ToString());
                }
            }
        }

        public void Refresh()
        {
            for (int i = 0; i < 15; i++)
            {
                Notify("Action" + i.ToString());
            }
        }


        private void ImageMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Image imageControl = (Image)sender;
                int imageNum = int.Parse(imageControl.Tag as String);
                controller.PerformAction(imageNum + 1);
            }
            else if (e.ClickCount == 1)
            {
                Image imageControl = (Image)sender;
                int imageNum = int.Parse(imageControl.Tag as String);
                if (SelectedNumber == imageNum)
                {
                    
                    return;
                }
                SelectedNumber = imageNum;
                Border parentBorder = imageControl.Parent as Border;
                if (parentBorder == null) return;
                parentBorder.BorderBrush = Brushes.Blue;
                if (SelectedImage != null)
                {
                    (SelectedImage.Parent as Border).BorderBrush = Brushes.DarkGray;
                }

                SelectedImage = imageControl;

                if (Actions[SelectedNumber] is LaunchAction)
                {
                    LaunchActionPanel.Visibility = Visibility.Visible;
                    TextStringActionPanel.Visibility = Visibility.Collapsed;
                    tbPath.Text = (Actions[SelectedNumber] as LaunchAction).ExePath;
                    tbArgs.Text = (Actions[SelectedNumber] as LaunchAction).Args;
                    tbTitleAction.Text = (Actions[SelectedNumber] as LaunchAction).Title;
                    tbNewProcessAction.SelectedItem =
                        (Actions[SelectedNumber] as LaunchAction).AlreadyRunningAction.ToString();
                }
                else if (Actions[SelectedNumber] is TextStringAction)
                {
                    LaunchActionPanel.Visibility = Visibility.Collapsed;
                    TextStringActionPanel.Visibility = Visibility.Visible;
                    tbValue.Text = (Actions[SelectedNumber] as TextStringAction).Value;
                    tbTitleTextString.Text = (Actions[SelectedNumber] as TextStringAction).Title;
                }
                else
                {
                    tbTitleAction.Text = tbTitleTextString.Text = tbValue.Text = tbPath.Text = tbArgs.Text = "";
                    LaunchActionPanel.Visibility = Visibility.Collapsed;
                    TextStringActionPanel.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void BrightnessSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (controller == null) return;
            int newVal = (int) e.NewValue;
            controller.SetDeviceBrightness(newVal);
            Properties.Settings.Default.Brightness = newVal;
            Properties.Settings.Default.Save();
        }

        private void discardButtonClick(object sender, RoutedEventArgs e)
        {
            if (Actions[SelectedNumber] is LaunchAction)
            {
                tbPath.Text = (Actions[SelectedNumber] as LaunchAction).ExePath;
                tbArgs.Text = (Actions[SelectedNumber] as LaunchAction).Args;
                tbTitleAction.Text = (Actions[SelectedNumber] as LaunchAction).Title;
                tbNewProcessAction.SelectedItem =
                    (Actions[SelectedNumber] as LaunchAction).AlreadyRunningAction.ToString();
            }
            else if (Actions[SelectedNumber] is TextStringAction)
            {
                tbValue.Text = (Actions[SelectedNumber] as TextStringAction).Value;
                tbTitleTextString.Text = (Actions[SelectedNumber] as TextStringAction).Title;
            }
        }

        public Icon GetBestIcon(String path)
        {
            IconExtractor ie = new IconExtractor(path);
            string fileName = ie.FileName;
            int iconCount = ie.Count;
            if (iconCount > 0)
            {
                Icon[] allIcons = ie.GetAllIcons();
                Icon ret = null;
                foreach (var icon in allIcons)
                {
                    Icon[] splitIcons = IconUtil.Split(icon);
                    Icon biggestIcon = splitIcons.OrderBy(x => x.Size.Height).Last();
                    if (ret == null || biggestIcon.Height > ret.Height) ret = biggestIcon;
                }

                return ret;
            }

            return null;
        }

        public static string FindExePath(string exe)
        {
            exe = exe.Trim(new[] {'"', ' '});
            if (string.IsNullOrEmpty(exe)) return "";
            exe = Environment.ExpandEnvironmentVariables(exe);
            if (!File.Exists(exe))
            {
                if (System.IO.Path.GetDirectoryName(exe) == String.Empty)
                {
                    foreach (string test in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';'))
                    {
                        string path = test.Trim();
                        if (!String.IsNullOrEmpty(path) && File.Exists(path = System.IO.Path.Combine(path, exe)))
                            return System.IO.Path.GetFullPath(path);
                    }
                }

                return exe;
            }
            return System.IO.Path.GetFullPath(exe);
        }

        private void saveButtonClick(object sender, RoutedEventArgs e)
        {
            if (Actions[SelectedNumber] is LaunchAction)
            {
                var fullPath = FindExePath(tbPath.Text);
                if (File.Exists(fullPath) && fullPath != (Actions[SelectedNumber] as LaunchAction).ExePath)
                {
                    var icon = GetBestIcon(fullPath);
                    if (icon != null)
                    {
                        //Bitmap bitmap = IconUtil.ToBitmap(icon);
                        Bitmap bitmap = icon.ToBitmap();
                        bitmap.Save("Temp.png");
                        controller.AddImage(SelectedNumber + 1, "Temp.png");

                    }
                }

                (Actions[SelectedNumber] as LaunchAction).ExePath = fullPath;
                (Actions[SelectedNumber] as LaunchAction).Args = tbArgs.Text;
                (Actions[SelectedNumber] as LaunchAction).Title = tbTitleAction.Text;
                var result = LaunchAction.ProcessRunningAction.FocusOldProcess;
                Enum.TryParse(tbNewProcessAction.SelectedItem.ToString(), out result);
                (Actions[SelectedNumber] as LaunchAction).AlreadyRunningAction = result;
                controller.Save();
            }
            else if (Actions[SelectedNumber] is TextStringAction)
            {
                (Actions[SelectedNumber] as TextStringAction).Value = tbValue.Text;
                (Actions[SelectedNumber] as TextStringAction).Title = tbTitleTextString.Text;
            }

            controller.LoadIcons();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            this.ShowInTaskbar = false;
            e.Cancel = true;
        }

        public void BringToFront()
        {
            this.Show();
            this.ShowInTaskbar = true;
            this.WindowState = WindowState.Normal;
            this.BringIntoView();
            this.Activate();
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            BringToFront();
        }


        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Click += notifyIcon_Click;
            var mI = new System.Windows.Forms.MenuItem("Exit Infinitton WPF");
            mI.Click += MenuItem_OnClick;
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(new []{ mI });
            notifyIcon.Icon = new System.Drawing.Icon("Icon16.ico");
            notifyIcon.Visible = true;
            brightnessSlider.Value = Properties.Settings.Default.Brightness;
            controller = new MainController(this);
            tbNewProcessAction.Items.Add(LaunchAction.ProcessRunningAction.NewProcess.ToString());
            tbNewProcessAction.Items.Add(LaunchAction.ProcessRunningAction.FocusOldProcess.ToString());
            tbNewProcessAction.Items.Add(LaunchAction.ProcessRunningAction.KillOldProcess.ToString());
        }

        private void MenuItem_OnClick(object sender, EventArgs e)
        {
            try
            {
                controller?.Kill();
            }
            catch
            {

            }


            Application.Current.Shutdown();
        }
    }
}
