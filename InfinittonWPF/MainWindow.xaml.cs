using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TsudaKageyu;
using WindowsInput.Native;
using Application = System.Windows.Application;
using Brushes = System.Windows.Media.Brushes;
using ContextMenu = System.Windows.Controls.ContextMenu;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using Image = System.Windows.Controls.Image;
using MenuItem = System.Windows.Controls.MenuItem;

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

        public String HotkeyActionPath
        {
            get { return "HotkeyIcon.png"; }
        }

        public String VersionString
        {
            get { return "Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
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
            set
            {
                Actions[0].IconPath = value;
                Notify("Action0");
            }
        }

        public String Action1
        {
            get { return Actions[1].IconPath; }
            set
            {
                Actions[1].IconPath = value;
                Notify("Action1");
            }
        }

        public String Action2
        {
            get { return Actions[2].IconPath; }
            set
            {
                Actions[2].IconPath = value;
                Notify("Action2");
            }
        }

        public String Action3
        {
            get { return Actions[3].IconPath; }
            set
            {
                Actions[3].IconPath = value;
                Notify("Action3");
            }
        }

        public String Action4
        {
            get { return Actions[4].IconPath; }
            set
            {
                Actions[4].IconPath = value;
                Notify("Action4");
            }
        }

        public String Action5
        {
            get { return Actions[5].IconPath; }
            set
            {
                Actions[5].IconPath = value;
                Notify("Action5");
            }
        }

        public String Action6
        {
            get { return Actions[6].IconPath; }
            set
            {
                Actions[6].IconPath = value;
                Notify("Action6");
            }
        }

        public String Action7
        {
            get { return Actions[7].IconPath; }
            set
            {
                Actions[7].IconPath = value;
                Notify("Action7");
            }
        }

        public String Action8
        {
            get { return Actions[8].IconPath; }
            set
            {
                Actions[8].IconPath = value;
                Notify("Action8");
            }
        }

        public String Action9
        {
            get { return Actions[9].IconPath; }
            set
            {
                Actions[9].IconPath = value;
                Notify("Action9");
            }
        }

        public String Action10
        {
            get { return Actions[10].IconPath; }
            set
            {
                Actions[10].IconPath = value;
                Notify("Action10");
            }
        }

        public String Action11
        {
            get { return Actions[11].IconPath; }
            set
            {
                Actions[11].IconPath = value;
                Notify("Action11");
            }
        }

        public String Action12
        {
            get { return Actions[12].IconPath; }
            set
            {
                Actions[12].IconPath = value;
                Notify("Action12");
            }
        }

        public String Action13
        {
            get { return Actions[13].IconPath; }
            set
            {
                Actions[13].IconPath = value;
                Notify("Action13");
            }
        }

        public String Action14
        {
            get { return Actions[14].IconPath; }
            set
            {
                Actions[14].IconPath = value;
                Notify("Action14");
            }
        }

        private MainController controller;
        private System.Windows.Forms.NotifyIcon notifyIcon = null;
        private WinEventDelegate dele;

        public MainWindow()
        {
            for (int i = 0; i < 15; i++)
            {
                _Actions.Add(new NullAction());
            }

            InitializeComponent();
            this.DataContext = this;
            dele = new WinEventDelegate(WinEventProc);
            IntPtr m_hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0,
                WINEVENT_OUTOFCONTEXT);

        }

        private void actionImageMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image image = e.Source as Image;
            DragDrop.DoDragDrop(image, image.Tag, DragDropEffects.All);
        }

        private void imageDrop(object sender, DragEventArgs e)
        {
            Image imageControl = (Image) sender;
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
                    case "HotkeyAction":
                        Actions[imageNum] = new HotkeyAction();
                        break;
                }

                controller.AddAction(imageNum + 1, Actions[imageNum]);
                Notify("Action" + imageNum.ToString());
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);
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
                Image imageControl = (Image) sender;
                int imageNum = int.Parse(imageControl.Tag as String);
                controller.PerformAction(imageNum + 1);
            }
            else if (e.ClickCount == 1)
            {
                Image imageControl = (Image) sender;
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
                    FolderActionPanel.Visibility = Visibility.Collapsed;
                    TextStringActionPanel.Visibility = Visibility.Collapsed;
                    HotkeyActionPanel.Visibility = Visibility.Collapsed;
                    tbPath.Text = (Actions[SelectedNumber] as LaunchAction).ExePath;
                    tbArgs.Text = (Actions[SelectedNumber] as LaunchAction).Args;
                    tbTitleAction.Text = (Actions[SelectedNumber] as LaunchAction).Title;
                    tbNewProcessAction.SelectedItem =
                        (Actions[SelectedNumber] as LaunchAction).AlreadyRunningAction.ToString();
                }
                else if (Actions[SelectedNumber] is TextStringAction)
                {
                    LaunchActionPanel.Visibility = Visibility.Collapsed;
                    FolderActionPanel.Visibility = Visibility.Collapsed;
                    TextStringActionPanel.Visibility = Visibility.Visible;
                    HotkeyActionPanel.Visibility = Visibility.Collapsed;
                    tbValue.Text = (Actions[SelectedNumber] as TextStringAction).Value;
                    tbTitleTextString.Text = (Actions[SelectedNumber] as TextStringAction).Title;
                }
                else if (Actions[SelectedNumber] is FolderAction)
                {
                    LaunchActionPanel.Visibility = Visibility.Collapsed;
                    FolderActionPanel.Visibility = Visibility.Visible;
                    TextStringActionPanel.Visibility = Visibility.Collapsed;
                    HotkeyActionPanel.Visibility = Visibility.Collapsed;
                    tbFolderCondition.Text = (Actions[SelectedNumber] as FolderAction).ExeConditionName;
                    tbTitleTextString.Text = (Actions[SelectedNumber] as FolderAction).Title;
                }
                else if (Actions[SelectedNumber] is HotkeyAction)
                {
                    LaunchActionPanel.Visibility = Visibility.Collapsed;
                    FolderActionPanel.Visibility = Visibility.Collapsed;
                    TextStringActionPanel.Visibility = Visibility.Collapsed;
                    HotkeyActionPanel.Visibility = Visibility.Visible;
                    tbValueHotkey.Text = (Actions[SelectedNumber] as HotkeyAction).ToString();
                    Modifiers = (Actions[SelectedNumber] as HotkeyAction).Modifiers;
                    MainKey = (Actions[SelectedNumber] as HotkeyAction).MainKey;
                }
                else
                {
                    tbTitleAction.Text = tbTitleTextString.Text = tbValue.Text = tbPath.Text = tbArgs.Text = "";
                    LaunchActionPanel.Visibility = Visibility.Collapsed;
                    TextStringActionPanel.Visibility = Visibility.Collapsed;
                    HotkeyActionPanel.Visibility = Visibility.Collapsed;
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
            else if (Actions[SelectedNumber] is FolderAction)
            {
                tbFolderCondition.Text = (Actions[SelectedNumber] as FolderAction).ExeConditionName;
                tbTitleTextString.Text = (Actions[SelectedNumber] as FolderAction).Title;
            }
            else if (Actions[SelectedNumber] is HotkeyAction)
            {
                tbValueHotkey.Text = (Actions[SelectedNumber] as HotkeyAction).ToString();
                tbTitleHotkey.Text = (Actions[SelectedNumber] as HotkeyAction).Title;
                Modifiers = (Actions[SelectedNumber] as HotkeyAction).Modifiers;
                MainKey = (Actions[SelectedNumber] as HotkeyAction).MainKey;
            }
        }

        public Icon GetBestIcon(String path)
        {
            if (!path.EndsWith(".exe") && !path.EndsWith(".dll")) return null;
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
            else if (Actions[SelectedNumber] is FolderAction)
            {
                (Actions[SelectedNumber] as FolderAction).ExeConditionName = tbFolderCondition.Text;
                (Actions[SelectedNumber] as FolderAction).Title = tbTitleTextString.Text;
            }
            else if (Actions[SelectedNumber] is HotkeyAction)
            {
                (Actions[SelectedNumber] as HotkeyAction).MainKey = MainKey;
                (Actions[SelectedNumber] as HotkeyAction).Modifiers = Modifiers;
                (Actions[SelectedNumber] as HotkeyAction).Title = tbTitleHotkey.Text;
            }

            controller.Save();
            controller.LoadIcons();
        }

        // Used for temp for hotkey until save is clicked.
        public List<VirtualKeyCode> Modifiers = new List<VirtualKeyCode>();
        public VirtualKeyCode MainKey = VirtualKeyCode.VK_A;

        private void PathButtonClick(object sender, RoutedEventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.DefaultExt = ".exe";
                ofd.Filter = "Exe files (*.exe, *.bat, *.lnk) | *.exe; *.bat; *.lnk";
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (Actions[SelectedNumber] is LaunchAction)
                    {
                        var fullPath = ofd.FileName;
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

                        tbPath.Text = fullPath;
                        (Actions[SelectedNumber] as LaunchAction).ExePath = fullPath;
                        controller.Save();
                    }
                }
            }
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
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(new[] {mI});
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
            Exit();
        }



        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild,
            uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
            WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            IntPtr handle = IntPtr.Zero;
            StringBuilder Buff = new StringBuilder(nChars);
            handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }

            return null;
        }

        public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild,
            uint dwEventThread, uint dwmsEventTime)
        {
            try
            {
                if (hwnd == IntPtr.Zero) return;
                uint pid;
                GetWindowThreadProcessId(hwnd, out pid);
                var process = Process.GetProcessById((int) pid);
                controller?.ProcessAppSwitchedFocus(process.ProcessName);
                Console.WriteLine("Switched to process: " + process.ProcessName);
            }
            catch
            {

            }
        }

        private void ImageKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (Actions[SelectedNumber] is NullAction) return;

                Actions[SelectedNumber] = new NullAction();
                controller.AddAction(SelectedNumber + 1, Actions[SelectedNumber]);
                Notify("Action" + SelectedNumber.ToString());
            }


        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void MenuButtonClick(object sender, RoutedEventArgs e)
        {
            MainDrawer.IsLeftDrawerOpen = !MainDrawer.IsLeftDrawerOpen;
        }

        private void deleteButtonClicked(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            if (mi != null)
            {
                ContextMenu cm = mi.Parent as ContextMenu;
                if (cm != null)
                {
                    Image g = cm.PlacementTarget as Image;
                    if (g != null)
                    {
                        int num = int.Parse(g.Tag.ToString());
                        if (Actions[num] is NullAction) return;

                        Actions[num] = new NullAction();
                        controller.AddAction(num + 1, Actions[num]);
                        Notify("Action" + num.ToString());
                    }
                }
            }


        }

        public Key[] IgnoreKeys =
        {
            Key.LeftAlt, Key.LeftCtrl, Key.LeftShift, Key.LWin, Key.RWin, Key.RightAlt, Key.RightCtrl, Key.RightShift
        };

        private void hotkeyTextboxKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (IgnoreKeys.Contains(e.Key)) return;

            Modifiers.Clear();

            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                Modifiers.Add(VirtualKeyCode.MENU);
                if (IgnoreKeys.Contains(e.SystemKey)) return;
            }
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) Modifiers.Add(VirtualKeyCode.CONTROL);
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) Modifiers.Add(VirtualKeyCode.SHIFT);
            if ((Keyboard.Modifiers & ModifierKeys.Windows) == ModifierKeys.Windows) Modifiers.Add(VirtualKeyCode.LWIN);

            MainKey = (VirtualKeyCode)KeyInterop.VirtualKeyFromKey(e.Key != Key.System ? e.Key : e.SystemKey);
            Console.WriteLine(Utils.GetKeyString(MainKey));

            tbValueHotkey.Text = Utils.GetKeysString(Modifiers, MainKey);
            e.Handled = true;
        }

        public void Exit()
        {
            try
            {
                controller.SetDeviceBrightness(0);
                controller?.Kill();
            }
            catch
            {

            }


            Application.Current.Shutdown();
        }

        private void exitButtonClicked(object sender, RoutedEventArgs e)
        {
            Exit();
        }

        private void importButtonClicked(object sender, RoutedEventArgs e)
        {
            controller.Import();
        }

        private void exportButtonClicked(object sender, RoutedEventArgs e)
        {
            controller.Export();
        }

        private void brightnessMinClicked(object sender, MouseButtonEventArgs e)
        {
            brightnessSlider.Value = 0;
        }

        private void brightnessMaxClicked(object sender, MouseButtonEventArgs e)
        {
            brightnessSlider.Value = 100;
        }
    }
}
