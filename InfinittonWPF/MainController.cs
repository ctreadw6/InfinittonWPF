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
        String FolderIconPath = Path.GetFullPath("Folder-Icon.png");
        public static String HomeIconPath = Path.GetFullPath("Home.png");
        Random random = new Random();

        public static HomeFolderAction HomeAction = new HomeFolderAction() { IconPath = HomeIconPath };

        public void handle(object s, USBInterface.ReportEventArgs a)
        {
            if (IgnoreReport) return;

            int val = a.Data[1] << 8 | a.Data[2];
            int buttonNum = ButtonMapper.GetButtonIndex((ushort)val) + 1;
            if (buttonNum <= 0) return;
            Console.WriteLine(string.Join(", ", a.Data));
            Console.WriteLine("Button " + buttonNum);
            PerformAction(buttonNum);
        }

        public void enter(object s, EventArgs a)
        {
            Console.WriteLine("device arrived");
        }
        public void exit(object s, EventArgs a)
        {
            Console.WriteLine("device removed");
        }

        public MainController(MainWindow _mainWindow)
        {
            if (!Directory.Exists("Images")) Directory.CreateDirectory("Images");
            mainWindow = _mainWindow;

            //allActions.TryAdd("1", new FolderAction());

            //allActions.TryAdd("1-2", new FolderAction());

            //allActions.TryAdd("1-2-3", new LaunchAction() { ExePath = @"C:\Users\ctrea\AppData\Roaming\Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar\File Explorer.lnk" });
            scanner = new DeviceScanner(0xffff, 0x1f40);
            scanner.DeviceArrived += enter;
            scanner.DeviceRemoved += exit;
            scanner.StartAsyncScan();
            device = new USBDevice(0xffff, 0x1f40, null, false, 31);
            //_ButtonDevice = HidDevices.Enumerate(0xffff, 0x1f40).FirstOrDefault();
            if (device == null) return;

            Console.WriteLine(device.GetProductString());

            // add handle for data read
            device.InputReportArrivedEvent += handle;
            // after adding the handle start reading
            device.StartAsyncRead();
            // can add more handles at any time
            device.InputReportArrivedEvent += handle;

            //Thread t = new Thread(ListenForButton);
            //t.Start();
            Load();
            LoadIcons();

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

            for (int i = i1; i < i2; i++)
            {
                allActions.TryAdd(Properties.Settings.Default.ActionsSetting[i], new FolderAction());
            }

            i1 = Properties.Settings.Default.ActionsSetting.IndexOf("[LaunchAction]") + 1;
            i2 = Properties.Settings.Default.ActionsSetting.Count;
            for (int i = i1; i < i2; i += 3)
            {
                var action = new LaunchAction();
                string path = Properties.Settings.Default.ActionsSetting[i];
                action.ExePath = Properties.Settings.Default.ActionsSetting[i + 1];
                action.Args = Properties.Settings.Default.ActionsSetting[i + 2];
                if (File.Exists("Images/" + path + ".png")) action.IconPath = "Images/" + path + ".png";

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
            }

            setting.Add("[LaunchAction]");
            foreach (var kvp in allActions.Where(x => x.Value.GetType() == typeof(LaunchAction)))
            {
                var action = kvp.Value as LaunchAction;
                setting.Add(kvp.Key);
                setting.Add(action.ExePath);
                setting.Add(action.Args);
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

                File.Copy(copyFromPath, path);
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

                        BitmapSource source = null;
                        //images[index].Dispatcher.Invoke((Action)delegate
                        //{
                        //    images[index].Source = actions[index].BMPImage;
                        //});

                        SendKeyFeature(index, actions[index]?.Icon);

                        currentList.RemoveAt(currentList.Count - 1);
                    }
                    else
                    {
                        actions.TryAdd(index, HomeAction);
                        //images[index].Dispatcher.Invoke((Action)delegate
                        //{
                        //    images[index].DataContext = actions[index];
                        //});
                        SendKeyFeature(index, actions[index]?.Icon);
                    }

                    mainWindow.Actions[index - 1] = actions[index];
                }

                mainWindow.Refresh();
                IgnoreReport = false;
            }
        }

        public void Start()
        {
            while (true)
            {
                device.Read();
                //_device.ReadReport(OnReport);
                Thread.Sleep(10);
            }
        }

        public void PerformAction(int button)
        {
            if (button > 0)
            {
                if (actions[button] != null)
                    actions[button].DoStuff(this, button);
                //_device.ReadFeatureData(out buf, 1);
            }
        }

        //private void OnReport(HidReport report)
        //{
        //    //if (!_device.IsConnected) return;
        //    //if (report.ReadStatus != HidDeviceData.ReadStatus.Success)
        //    //{

        //    //    return;
        //    //}
            

        //    //_device.WriteReport(report);
        //    int val = report.Data[0] << 8 | report.Data[1];
        //    int buttonNum = ButtonMapper.GetButtonIndex((ushort)val) + 1;
        //    PerformAction(buttonNum);
        //}

        private void DeviceRemovedHandler()
        {
            Console.WriteLine("Removed");
        }

        private void DeviceAttachedHandler()
        {
            Console.WriteLine("Attached");
            //_device.ReadReport(OnReport);
        }

        public void SetDeviceBrightness(int val)
        {
            //lock (lockObj)
            {
                byte[] buf = new byte[] { 0x11, (byte)val };
                device.SendFeatureReport(buf);
                //_device.WriteFeatureData(buf);
            }
        }

        public void SendKeyFeature(int key, String path)
        {
            if (File.Exists(path))
            {
                SendKeyFeature(key, Bitmap.FromFile(path));
            }
        }

        public void SendKeyFeature(int key, System.Drawing.Image image = null)
        {
            int numTimes = 3;
            //lock (lockObj)
            {
                byte[] ret = new byte[34];
                byte[] buf;


                buf = PictureConverter.GetBuffer(image);
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
    }
}
