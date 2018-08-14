using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USBInterface;

namespace TestUSBInterface
{
    class Program
    {

        public static void handle(object s, USBInterface.ReportEventArgs a)
        {
            Console.WriteLine(string.Join(", ", a.Data));
        }

        public static void enter(object s, EventArgs a)
        {
            Console.WriteLine("device arrived");
        }
        public static void exit(object s, EventArgs a)
        {
            Console.WriteLine("device removed");
        }

        static void Main(string[] args)
        {
            // setup a scanner before hand
            DeviceScanner scanner = new DeviceScanner(0x4d8, 0x3f);
            scanner.DeviceArrived += enter;
            scanner.DeviceRemoved += exit;
            scanner.StartAsyncScan();
            Console.WriteLine("asd");

            // this should probably happen in enter() function
            try
            {
                // this can all happen inside a using(...) statement
                USBDevice dev = new USBDevice(0x4d8, 0x3f, null, false, 31);

                Console.WriteLine(dev.Description());

                // add handle for data read
                dev.InputReportArrivedEvent += handle;
                // after adding the handle start reading
                dev.StartAsyncRead();
                // can add more handles at any time
                dev.InputReportArrivedEvent += handle;

                // write some data
                byte[] data = new byte[32];
                data[0] = 0x00;
                data[1] = 0x23;
                dev.Write(data);

                dev.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadKey();
        }
    }


}
