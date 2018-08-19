using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using WindowsInput;
using InfinittonWPF.Properties;
using Microsoft.Win32;
using USBInterface;

namespace InfinittonWPF
{
    public class MainController
    {
        private DeviceScanner scanner;
        private USBDevice device;
        private static readonly object lockObj = new object();
        Stack<int> CurrentFolderDir = new Stack<int>();
        ConcurrentDictionary<int, IButtonPressAction> actions = new ConcurrentDictionary<int, IButtonPressAction>();
        ConcurrentDictionary<string, IButtonPressAction> allActions = new ConcurrentDictionary<string, IButtonPressAction>();
        private MainWindow mainWindow;
        

        public static String HomeIconPath = Path.GetFullPath("Home.png");
        Random random = new Random();
        

        public static HomeFolderAction HomeAction = new HomeFolderAction() { IconPath = HomeIconPath };

        DateTime lastButtonPressTime = DateTime.MinValue;
        private int lastButtonPressNum = -1;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int cmd);

        // Maximize the window
        public static bool ShowWindow(IntPtr hWnd)
        {
            return ShowWindow(hWnd, 3);
        }

        public bool CheckDevice()
        {
            return device != null && device.isOpen;
        }

        public void handle(object s, USBInterface.ReportEventArgs a)
        {
            if (IgnoreReport) return;

            int val = a.Data[1] << 8 | a.Data[2];
            int buttonNum = ButtonMapper.GetButtonIndex((ushort)val) + 1;
            if (buttonNum <= 0) return;

            // Not sure why each report happens twice, but this will try and weed them out
            if (lastButtonPressNum == buttonNum && DateTime.Now.Subtract(lastButtonPressTime).TotalSeconds < 1) return;
            lastButtonPressTime = DateTime.Now;
            lastButtonPressNum = buttonNum;

            Console.WriteLine(string.Join(", ", a.Data));
            Console.WriteLine("Button " + buttonNum);
            PerformAction(buttonNum);
        }

        public void enter(object s, EventArgs a)
        {
            device = new USBDevice(0xffff, 0x1f40, null, false, 31);

            if (device == null) return;

            Console.WriteLine(device.GetProductString());

            // add handle for data read
            device.InputReportArrivedEvent += handle;
            // after adding the handle start reading
            device.StartAsyncRead();
            // can add more handles at any time
            device.InputReportArrivedEvent += handle;

            Load();
            LoadIcons();
        }
        public void exit(object s, EventArgs a)
        {
            device = null;
        }

        public MainController(MainWindow _mainWindow)
        {
            if (!Directory.Exists("Images")) Directory.CreateDirectory("Images");
            mainWindow = _mainWindow;

            scanner = new DeviceScanner(0xffff, 0x1f40);
            scanner.DeviceArrived += enter;
            scanner.DeviceRemoved += exit;
            scanner.StartAsyncScan();


            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (!CheckDevice()) return;
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                SetDeviceBrightness(0);
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                SetDeviceBrightness(Properties.Settings.Default.Brightness);
            }
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (!CheckDevice()) return;
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    SetDeviceBrightness(Properties.Settings.Default.Brightness);
                    break;
                case PowerModes.Suspend:
                    SetDeviceBrightness(0);
                    break;
            }
        }

        public void ChangeFolder(int folderNum)
        {
            if (folderNum < 0)
            {
                // Go up a dir
                CurrentFolderDir.Pop();
            }
            else
            {
                CurrentFolderDir.Push(folderNum);
            }
            LoadIcons();
        }

        public void Load()
        {
            int i1, i2;
            if (Properties.Settings.Default.ActionsSetting == null || Properties.Settings.Default.ActionsSetting.Count <= 0) return;

            i1 = Properties.Settings.Default.ActionsSetting.IndexOf("[FolderActions]") +1;
            i2 = Properties.Settings.Default.ActionsSetting.IndexOf("[LaunchAction]");

            for (int i = i1; i < i2; i += 2)
            {
                var action = new FolderAction();
                string path = Properties.Settings.Default.ActionsSetting[i];
                action.Title = Properties.Settings.Default.ActionsSetting[i + 1];
                allActions.TryAdd(path, action);
                
            }

            i1 = Properties.Settings.Default.ActionsSetting.IndexOf("[LaunchAction]") + 1;
            i2 = Properties.Settings.Default.ActionsSetting.IndexOf("[StringActions]");
            for (int i = i1; i < i2; i += 5)
            {
                var action = new LaunchAction();
                string path = Properties.Settings.Default.ActionsSetting[i];
                action.Title = Properties.Settings.Default.ActionsSetting[i + 1];
                action.ExePath = Properties.Settings.Default.ActionsSetting[i + 2];
                action.Args = Properties.Settings.Default.ActionsSetting[i + 3];
                var result = LaunchAction.ProcessRunningAction.FocusOldProcess;
                Enum.TryParse(Properties.Settings.Default.ActionsSetting[i + 4], out result);
                action.AlreadyRunningAction = result;
                if (File.Exists("Images/" + path + ".png")) action.IconPath = "Images/" + path + ".png";

                allActions.TryAdd(path, action);
            }

            i1 = Properties.Settings.Default.ActionsSetting.IndexOf("[StringActions]") + 1;
            i2 = Properties.Settings.Default.ActionsSetting.Count;
            for (int i = i1; i < i2; i += 3)
            {
                var action = new TextStringAction();
                string path = Properties.Settings.Default.ActionsSetting[i];
                action.Title = Properties.Settings.Default.ActionsSetting[i + 1];
                action.Value = Properties.Settings.Default.ActionsSetting[i + 2];
                allActions.TryAdd(path, action);
            }
        }

        public void Save()
        {
            StringCollection setting = new StringCollection();
            setting.Add("[FolderActions]");
            foreach (var kvp in allActions.Where(x => x.Value.GetType() == typeof(FolderAction)))
            {
                setting.Add(kvp.Key);
                setting.Add(kvp.Value.Title);
            }

            setting.Add("[LaunchAction]");
            foreach (var kvp in allActions.Where(x => x.Value.GetType() == typeof(LaunchAction)))
            {
                var action = kvp.Value as LaunchAction;
                setting.Add(kvp.Key);
                setting.Add(action.Title);
                setting.Add(action.ExePath);
                setting.Add(action.Args);
                setting.Add(action.AlreadyRunningAction.ToString());
            }

            setting.Add("[StringActions]");
            foreach (var kvp in allActions.Where(x => x.Value.GetType() == typeof(TextStringAction)))
            {
                var action = kvp.Value as TextStringAction;
                setting.Add(kvp.Key);
                setting.Add(action?.Title);
                setting.Add(action?.Value);
            }

            Properties.Settings.Default.ActionsSetting = setting;
            Properties.Settings.Default.Save();
        }

        public void AddImage(int index, String copyFromPath)
        {
            lock (lockObj)
            {
                string fullPath = string.Join("-", CurrentFolderDir.Reverse()) + "-" + index.ToString();
                fullPath = fullPath.Trim(new[] { '-' });
                string path = System.IO.Path.Combine("Images", fullPath + ".png");

                File.Copy(copyFromPath, path, true);
                IButtonPressAction action;
                if (allActions.ContainsKey(fullPath) && allActions.TryGetValue(fullPath, out action) && action.GetType() != typeof(NullAction))
                {
                    allActions[fullPath].IconPath = path;
                    LoadIcons();
                }

            }

        }

        public void AddAction(int index, IButtonPressAction action)
        {
            lock (lockObj)
            {
                string fullPath = string.Join("-", CurrentFolderDir.Reverse()) + "-" + index.ToString();
                fullPath = fullPath.Trim(new[] { '-' });
                if (allActions.ContainsKey(fullPath)) allActions[fullPath] = action;
                else allActions.TryAdd(fullPath, action);

                string path = System.IO.Path.Combine("Images", fullPath + ".png");

                File.Copy(action.IconPath, path, true);
                action.IconPath = path;
                Save();
            }
            LoadIcons();
        }

        public bool IgnoreReport = false;

        public void LoadIcons()
        {
            if (!CheckDevice()) return;
            lock (lockObj)
            {
                IgnoreReport = true;
                // Randomly set button images. This sort of helps to not see the delay when setting the images.
                List<int> randomList = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }.OrderBy(x => random.Next()).ToList();
                var currentList = CurrentFolderDir.Reverse().ToList();
                actions.Clear();
                for (int i = 1; i <= 15; i++)
                {
                    int index = randomList.First();
                    randomList.RemoveAt(0);
                    if (index < 15 || (index == 15 && CurrentFolderDir.Count == 0))
                    {
                        currentList.Add(index);
                        if (allActions.ContainsKey(string.Join("-", currentList)))
                        {
                            actions.TryAdd(index, allActions[string.Join("-", currentList)]);
                        }
                        else
                        {
                            actions.TryAdd(index, new NullAction());
                        }

                        SendKeyFeature(index, actions[index]);

                        currentList.RemoveAt(currentList.Count - 1);
                    }
                    else
                    {
                        actions.TryAdd(index, HomeAction);
                        SendKeyFeature(index, actions[index]);
                    }

                    mainWindow.Actions[index - 1] = actions[index];
                }

                mainWindow.Refresh();
                IgnoreReport = false;
            }
        }

        public void PerformAction(int button)
        {
            try
            {
                if (button > 0 && actions.ContainsKey(button))
                {
                    if (actions[button] != null)
                        actions[button].DoStuff(this, button);
                    //_device.ReadFeatureData(out buf, 1);
                }
            }
            catch
            {
                // todo show some sort of alert here.
            }
        }

        public void SetDeviceBrightness(int val)
        {
            if (!CheckDevice()) return;
            //lock (lockObj)
            {
                byte[] buf = new byte[] { 0x11, (byte)val };
                device.SendFeatureReport(buf);
                //_device.WriteFeatureData(buf);
            }
        }

        public void SendKeyFeature(int key, IButtonPressAction action)
        {
            if (!CheckDevice()) return;
            int numTimes = 3;
            //lock (lockObj)
            {
                byte[] ret = new byte[34];
                byte[] buf;


                buf = PictureConverter.GetBuffer(action);
                device.Write(buf.Take(8017).ToArray());
                device.Write(buf.Skip(8017).ToArray());

                buf = new byte[] { 0x12, 0x01, 0x00, 0x00, (byte)key, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xf6, 0x3c,
                                      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

                for (int i = 0; i < numTimes; i++)
                {
                    //_device.WriteFeatureData(buf);
                    //_device.ReadFeatureData(out ret);
                    Thread.Sleep(1);
                    device.SendFeatureReport(buf);
                    return;
                    //device.GetFeatureReport(ret);
                }
            }
        }

        internal void Kill()
        {
            scanner?.StopAsyncScan();
            device?.StopAsyncRead();
        }
    }
}
