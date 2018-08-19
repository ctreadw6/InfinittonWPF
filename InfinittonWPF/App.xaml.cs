using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Shell;

namespace InfinittonWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        private const string Unique = "{28B3DD06-F3EC-4166-9363-3EFF7181E62C}";

        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                var application = new App();
                application.InitializeComponent();
                
                application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }

             
        }

        //public static void CreateShortcut()
        //{
        //    string fileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        //    using (ShellLink shortcut = new ShellLink())
        //    {
        //        shortcut.Target = fileName;
        //        shortcut.WorkingDirectory = System.IO.Directory.GetCurrentDirectory();
        //        shortcut.Description = "Infinitton WPF";
        //        shortcut.DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
        //        shortcut.Save(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "InfinittonWPF.lnk"));
        //    }
        //}

#region ISingleInstanceApp Members
        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // Bring window to foreground
            if (this.MainWindow.WindowState == WindowState.Minimized)
            {
                this.MainWindow.WindowState = WindowState.Normal;
            }

            this.MainWindow.Activate();

            return true;
        }
#endregion
    }
}
