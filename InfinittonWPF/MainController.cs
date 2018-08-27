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
using WindowsInput.Native;
using System.Xml.Linq;
using System.Xml;
using System.IO.Compression;

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

        public string SaveFileName = "ButtonLayout.xml";
        

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

            mainWindow?.Dispatcher.Invoke(() => { mainWindow.Title = "Infinitton WPF - Device Connected"; });

            Console.WriteLine(device.GetProductString());

            // add handle for data read
            device.InputReportArrivedEvent += handle;
            // after adding the handle start reading
            device.StartAsyncRead();
            // can add more handles at any time
            device.InputReportArrivedEvent += handle;

            SetDeviceBrightness(Properties.Settings.Default.Brightness);

            
            LoadIcons();
        }
        public void exit(object s, EventArgs a)
        {
            device = null;
            mainWindow?.Dispatcher.Invoke(() => { mainWindow.Title = "Infinitton WPF - Device Disconnected"; });
        }

        public MainController(MainWindow _mainWindow)
        {
            if (!Directory.Exists("Images")) Directory.CreateDirectory("Images");
            
            mainWindow = _mainWindow;
            Load();
            LoadIcons();

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

        public void ChangeFolder(String absolutePath)
        {
            CurrentFolderDir.Clear();
            foreach (var dir in absolutePath.Split(new[] {'-'}))
            {
                CurrentFolderDir.Push(int.Parse(dir));
            }
            LoadIcons();
        }

        public void Load()
        {
            allActions.Clear();
            if (!File.Exists(SaveFileName)) return;
            XDocument doc = XDocument.Load(SaveFileName);
            foreach (XElement elem in doc.Element("root").Elements())
            {
                string path = "";
                IButtonPressAction action = null;
                switch (elem.Name.LocalName)
                {
                    case "FolderAction":
                        FolderAction folderAction = new FolderAction();
                        action = folderAction;
                        path = elem.Attribute("Key").Value.ToString();
                        folderAction.Title = elem.Attribute("Title").Value.ToString();
                        folderAction.ExeConditionName = elem.Attribute("ExeCondition").Value.ToString();
                        allActions.TryAdd(path, folderAction);
                        break;
                    case "LaunchAction":
                        LaunchAction launchAction = new LaunchAction();
                        action = launchAction;
                        path = elem.Attribute("Key").Value.ToString();
                        launchAction.Title = elem.Attribute("Title").Value.ToString();
                        launchAction.ExePath = elem.Attribute("ExePath").Value.ToString();
                        launchAction.Args = elem.Attribute("Args").Value.ToString();
                        var result = LaunchAction.ProcessRunningAction.FocusOldProcess;
                        Enum.TryParse(elem.Attribute("AlreadyRunningAction").Value.ToString(), out result);
                        launchAction.AlreadyRunningAction = result;
                        allActions.TryAdd(path, launchAction);
                        break;
                    case "StringAction":
                        TextStringAction textAction = new TextStringAction();
                        action = textAction;
                        path = elem.Attribute("Key").Value.ToString();
                        textAction.Title = elem.Attribute("Title").Value.ToString();
                        textAction.Value = elem.Attribute("Value").Value.ToString();
                        allActions.TryAdd(path, textAction);
                        break;
                    case "HotkeyAction":
                        HotkeyAction hotkeyAction = new HotkeyAction();
                        action = hotkeyAction;
                        path = elem.Attribute("Key").Value.ToString();
                        hotkeyAction.Title = elem.Attribute("Title").Value.ToString();
                        for (int i = 0; i < 3; i++)
                        {
                            VirtualKeyCode key = (VirtualKeyCode)int.Parse(elem.Attribute("Mod" + i).Value.ToString());
                            if (key != 0)
                                hotkeyAction.Modifiers.Add(key);
                        }

                        hotkeyAction.MainKey = (VirtualKeyCode)int.Parse(elem.Attribute("MainKey").Value.ToString());
                        allActions.TryAdd(path, hotkeyAction);
                        break;
                }
                if (File.Exists("Images/" + path + ".png")) action.IconPath = "Images/" + path + ".png";
            }
        }

        public void Save()
        {
            XDocument doc = new XDocument();
            XElement root = new XElement("root");
            doc.Add(root);
            foreach (var kvp in allActions.Where(x => x.Value.GetType() == typeof(FolderAction)))
            {
                XElement elem = new XElement("FolderAction");
                var action = kvp.Value as FolderAction;
                elem.SetAttributeValue("Key", kvp.Key);
                elem.SetAttributeValue("Title", action.Title);
                elem.SetAttributeValue("ExeCondition", action.ExeConditionName ?? "");
                root.Add(elem);
            }

            foreach (var kvp in allActions.Where(x => x.Value.GetType() == typeof(LaunchAction)))
            {
                XElement elem = new XElement("LaunchAction");
                var action = kvp.Value as LaunchAction;
                elem.SetAttributeValue("Key", kvp.Key);
                elem.SetAttributeValue("Title", action.Title);
                elem.SetAttributeValue("ExePath", action.ExePath);
                elem.SetAttributeValue("Args", action.Args);
                elem.SetAttributeValue("AlreadyRunningAction", action.AlreadyRunningAction.ToString());
                root.Add(elem);
            }

            foreach (var kvp in allActions.Where(x => x.Value.GetType() == typeof(TextStringAction)))
            {
                XElement elem = new XElement("StringAction");
                var action = kvp.Value as TextStringAction;

                elem.SetAttributeValue("Key", kvp.Key);
                elem.SetAttributeValue("Title", action.Title);
                elem.SetAttributeValue("Value", action.Value);
                root.Add(elem);
            }

            foreach (var kvp in allActions.Where(x => x.Value.GetType() == typeof(HotkeyAction)))
            {
                XElement elem = new XElement("HotkeyAction");
                var action = kvp.Value as HotkeyAction;
                elem.SetAttributeValue("Key", kvp.Key);
                elem.SetAttributeValue("Title", action.Title);
                for (int i = 0; i < 3; i++)
                {
                    if (action?.Modifiers.Count > i)
                    {
                        elem.SetAttributeValue("Mod" + i, ((int)action?.Modifiers[i]).ToString());
                    }
                    else
                    {
                        elem.SetAttributeValue("Mod" + i, "0");
                    }
                }
                elem.SetAttributeValue("MainKey", ((int)action?.MainKey).ToString());
                root.Add(elem);
            }

            doc.Save(SaveFileName);
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

        public void ProcessAppSwitchedFocus(String processName)
        {
            foreach (var kvp in allActions)
            {
                if (kvp.Value is FolderAction && (((FolderAction)kvp.Value).ExeConditionName == processName || Path.GetFileNameWithoutExtension(((FolderAction)kvp.Value).ExeConditionName) == processName))
                {
                    ChangeFolder(kvp.Key);
                }
            }
        }

        internal void Export()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Zip config File (*.zip) | *.zip";
            sfd.DefaultExt = ".zip";
            if (sfd.ShowDialog() ?? false)
            {
                if (File.Exists(sfd.FileName)) File.Delete(sfd.FileName);
                using (var archive = ZipFile.Open(sfd.FileName, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(SaveFileName, SaveFileName);
                    foreach (var file in Directory.EnumerateFiles("Images/"))
                    {
                        string fname = "Images/" + Path.GetFileName(file);
                        archive.CreateEntryFromFile(fname, fname);
                    }
                }
            }
        }

        internal void Import()
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Zip config File (*.zip) | *.zip";
                ofd.DefaultExt = ".zip";
                if (ofd.ShowDialog() ?? false)
                {
                    if (!File.Exists(ofd.FileName)) return;
                    using (var archive = ZipFile.Open(ofd.FileName, ZipArchiveMode.Read))
                    {
                        archive.GetEntry(SaveFileName);

                        for (int i = 0; i < 15; i++) mainWindow.Actions[i] = new NullAction(); // Clear out binding stuff
                        mainWindow.Refresh();

                        Directory.Delete("Images", true);
                        Directory.CreateDirectory("Images");

                        foreach (var entry in archive.Entries)
                        {
                            entry.ExtractToFile(entry.Name, true);
                        }
                    }
                    Load();
                    LoadIcons();
                }
            }
            catch (Exception exc)
            {

            }
        }
    }
}
