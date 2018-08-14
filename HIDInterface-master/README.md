# HIDInterface
C# Wrapper of HIDAPI from signal11, wrapper multiplatform, used for interfaction with generic HID Devices, USB or bluetooth

This is a fork of a project that appeared to be abandoned by its creator.

The code is completely rewritten.

It also includes some ideas from the code found in this SO question:
http://stackoverflow.com/questions/15368376/options-for-hidapi-on-os-x


# Getting started
```csharp
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

```

## Common question



### Using HIDAPI, how can you query the raw report descriptor?

This is from stack overflow:
http://stackoverflow.com/questions/17706853/using-hidapi-how-can-you-query-the-raw-report-descriptor

Answer: HIDAPI does not provide functions for getting or parsing the report descriptor. 
Since HIDAPI is for talking to a custom devices, these devices will likely contain all 
or mostly vendor-defined report items anyway.

So this implies that you should not use hidapi to work with printers/keyboards whose discriptors are not know beforehand.
For that you should look into the libusb project.

### Why is it called USBInterface if its a hid wrapper and can not really work with generic usb?
Thats what the original maintainer called it. One day I might change the name to HIDInterface.

## Gotchas

First of make sure your application is the same bitness as the compiled dll (32-bit aka 86) / (64-bit aka x64)

Because of the naming differences for hidapi dynamic library on all the platforms, this wrapper uses
"hidapi" as the dll name and let windows or mono try to handle extension resolution. But unfortunately
this means that it is your job to put either hidapi.dll (windows) or hidapi.so (linux) file in 
the solution directory together with USBInterface.dll (this library).

When you think you have set everything up the simplest way to check that things are working is to call DeviceScanner.ScanOnce function.

When setting this up on linux remember that linux by default requiers root permission to read and write to device. The DeviceScanner class should work regardless of root access.

On linux you would probably want to set up a udev rule so that you device is automatically remounted and at members of some group (like 'devices') can access the device without root.


When reading on windows, even if Report IDs are not used, during read function 
windows still sticks in the first command byte with 0x00 value. This must concern 
you _only_ if you pass a custom reportLen to the Read function. 
For reports that dont use Report IDs, you should set the DefaultInputReportLength in the constructor and 
this difference of windows to other systems will be handled tansparently and consistently.

(to me this ironically seems pretty logical, because the output and feature reports make 
use of this command byte, would have been easier to just include it everywhere, BUT this is not by the USB HID spec,)


According to the USB spec no item length should be larger than 32 bits (where item length is Report Size * Report Count)
so on linux there is a redundant check in hid-code.c that goes something like this:
http://lxr.free-electrons.com/source/drivers/hid/hid-core.c#L390

Over the years the value has changed from 32 -> 96 (or 92, cannot remember) -> 128, becasue the 
reality is: there are uncompliant USB devices! If you are connecting a hid device wich fails with 
`invalid report_size` error, you can increase that limit and recompile your kernel, then it will work. 
And file a bug too! Because that check is there just for protocol reasons and since kernel devs agreed 
that 32 does not work, it is then completely redundant!


And when your device is opened with USBDevice the DeviceScanner will think that the device is disconnected,
because it is busy. The moral is: the DeviceScanner is not perfect.



If you have a gui app and you attach an event handler to USBDevice events make sure you use Invoke or BeginInvoke in your handler function.
like so:
```csharp
// some place assign the handler
dev.InputReportReceivedHandler += this.DataHandler;

// ....

private void DataHandler(object sender, InputReportEventArgs args)
{
    if (this.InvokeRequired)
    {
        try 
        {
            this.Invoke(new Action<object, InputReportEventArgs>(DataHandler), sender, args);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        return;
    }

    // body of your method
}
```

# How to setup udev rule
You want to do this so that your programs can access the device without having to use sudo.

So first you should run lsusb to get device id
```
$ sudo lsusb
Bus 003 Device 001: ID 1d6b:0001 Linux Foundation 1.1 root hub
Bus 002 Device 001: ID 1d6b:0002 Linux Foundation 2.0 root hub
Bus 004 Device 013: ID 1234:5678 MyDevice
```

Since we are not working with just usb, but with hid devices we must be more specific.
The best way to find the path to your hid device is to run the hidtest program.
If you installed hidapi from the repo and hence dont have the hidtest program,
my advice is: now is a good time to download the source. Because you want to know
the exact path as hidapi sees it.

Anyway the hidtest program will list all hid devices and their paths such as
```
sudo ./hidtest-hidraw

Device Found
  type: 1234 5678
  path: /dev/hidraw3
  serial_number: 0
  Manufacturer: Nice Guy
  Product:      MyDevice from Nice Guy
  Release:      100
  Interface:    0
```

Now you ask udev what it knows about the device and you put in the "/dev/hidraw3":
```
sudo udevadm info -a -p $(udevadm info -q path -n /dev/hidraw3)
```

From its output you build a udev rule and after reboot all should work.
Important: when building the file make sure to query `SUBSYSTEM=="hidraw"` and not `"usb"`
Oh and `SUBSYSTEM` != `SUBSYSTEMS` <- watch the trailing `S`
Example rule file:
```
sudo cat udev/rules.d/xy-mydevice.rules
SUBSYSTEM=="hidraw", ATTRS{idVendor}=="1234", ATTRS{idProduct}=="5678", GROUP="john", MODE="0660"
```

Then to test you can run `udevadm monitor` in one console to view all device paths
and in another:
```
# check your udev rule
usedadm test $(udevadm info -q path -n /dev/hidraw3)
# force reread config
udevadm control --reload
# you should re-plug the device to be extra sure
udevadm trigger
```


# Important things for development

## How to understand HID

You will need to use some sort of a usb sniffer to at the very least check out the various types of reports.
On windows I can recommend USBlyzer.
For linux I can recommend wireshark (it has some plugin for usb sniffing)

THen if there is only one website you will read about HID this should be it:
http://www.usbmadesimple.co.uk/ums_5.htm

And you can find various HID related specs here:
http://www.usb.org/developers/hidpage

Both of the above are extremely good sources.

Talks about report descriptors, same as the specification but in normal english:
http://eleccelerator.com/tutorial-about-usb-hid-report-descriptors/

On linux by default lsusb is unable to show you the report descriptor even with sudo. Before that you need to unbind the device. But hid devices are not different to usual usb devices and they must be unbound from the hid-generic driver using their HID ID.

source for example: http://unix.stackexchange.com/questions/12005/how-to-use-linux-kernel-driver-bind-unbind-interface-for-usb-hid-devices#13035

Example: dmesg shows:
```
hid-generic 0003:046D:C229.0036: input,hiddev0,hidraw4: USB HID v1.11 Keypad [Logitech G19 Gaming Keyboard] on usb-0000:00:13.2-3.2/input1
```
Then you use 0003:046D:C229.0036 as HID device id and
```
echo -n "0003:046D:C229.0036" > /sys/bus/hid/drivers/hid-generic/unbind
```
To bind back just reinsert device or
```
echo -n "0003:046D:C229.0036" > /sys/bus/hid/drivers/hid-generic/bind
```




## How to work with unamanged code and import unamanaged DLL


1) You must use:

```
[DllImport(DLL_FILE_NAME, CallingConvention = CallingConvention.Cdecl)]
```

Also look into setting the [CharSet = CharSet.Ansi] when dealing with strings strings.

2) See how the C# types map to C types.
Primitive types map readily!
(int, short, ushort)
The only issue might be that long in C++ is 32-bit, so you should define all such instances as int in your C# code

3) passing strings:
````
int takes_string(char* s);
int takes_string(const char* s);
int takes_string(const unsigned char* s);
int takes_string(wchar_t* ws);
```
(and other similar variations...);

For all of them you should use: 
```
[DLLImport("...")]
private static extern int takes_string([In] string s)
```

The [In] attribute tells the marshaler that this is passed into a function and not returned.

4) Getting strings back:
```
int fills_string(char* pBuffer);
int fills_string(wchar_t* pBuffer);
```

For these ones use:
```
[DLLImport("...")]
private static extern int fills_string(StringBuilder sb);
```

Call it like this in c# (use the StringBuilder rather than string class on c# side):

```
var sb = new StringBuilder(255);
fills_string(sb);
Console.WriteLine(sb.ToString())
```

You use StringBuilder with char* and string with const char*.

Refer to the table for how C# types map onto C types:
https://msdn.microsoft.com/en-us/library/fzhhdwae(v=vs.110).aspx


Auto-marshaling:
This is code without auto-marshaling:
in C++ i have a function:

```
void GetImage(void* ptr)
{
    ....
}
```

and in C#:

```
[DllImport("libPylonInterface.so")]
private static extern void GetImage(IntPtr ptr);

public static void GetImage(out byte[] arr)
{
    //allocating unmanaged memory for byte array
    IntPtr ptr = Marshal.AllocHGlobal (_width * _height);
    //and "copying" image data to this pointer
    GetImage (ptr);

    arr = new byte[_width * _height];
    //copying from unmanaged to managed memory
    Marshal.Copy (ptr, arr, 0, _width * _height);
    Marshal.FreeHGlobal(ptr);
}
```

So we have to do many memory manipulations in c#.

Now this is with auto-marshaling:

```
[DllImport("libPylonInterface.so")]
private static extern void GetImage([Out] byte[] arr);

....

arr = new byte[_width * _height];
// now the arr is filled with data
GetImage(arr);
```

This also avoids the second memory copy because the marshaller will pin the managed array and pass its address to the unmanaged code. 
The unmanaged code can then populate the managed memory directly.


Manually managing memory in C#:
If for example you must pass a double** to the C function. One way to handle that is to manually allocate and marshal unmanaged memory. 
You cannot use a double[,] because that simply does not map to double*[].
Then in C# code you will have things like this (to setup double** shich is to be passed to a C function):

```
IntPtr[] CreateUnmanagedArrays(double[][] arr)
{
    IntPtr[] result = new IntPtr[arr.Length];
    for (int i=0; i<arr.Length; i++)
    {
        result[i] = Marshal.AllocCoTaskMem(arr[i].Length*sizeof(double));
        Marshal.Copy(arr[i], 0, result[i], arr[i].Length);
    }
    return result;
}

void DestroyUnmanagedArrays(IntPtr[] arr)
{
    for (int i=0; i<arr.Length; i++)
    {
        Marshal.FreeCoTaskMem(arr[i]);
        arr[i] = IntPtr.Zero;
    }
}
```


When things get too complex consider:
1) Stop auto-marshalling stuff, start using IntPtr and use Marshal class to move memory around yourself.
2) Use unsafe blocks.


Example with no auto-marshalling:
```
To call this C function:
char * GetDir(char* path ) {
    // whatever
    return path;
}

You use in C#:
[DllImport("your.dll", CharSet = CharSet.Ansi)]
IntPtr GetDir(StringBuilder path);
```

And then when you get the IntPtr you pass it to Marshal.PtrToStringAnsi() function in order to get a C# string back.

Example with unsafe blocks:
The C / C++ code:
```
int API_ReadFile(const wchar_t* filename, DataStruct** outData)
{
    *outData = new DataStruct();
    (*outData)->data = (unsigned char*)_strdup("hello");
    (*outData)->len = 5;
    return 0;
}

void API_Free(DataStruct** pp)
{
    free((*pp)->data);
    delete *pp;
    *pp = NULL;
}

```

The C# code to access those functions are as follows:

```
[StructLayout(LayoutKind.Sequential)]
struct DataStruct
{
    public IntPtr data;
    public int len;
};

[DllImport("ReadFile.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
unsafe private static extern int API_ReadFile([MarshalAs(UnmanagedType.LPWStr)]string filename, DataStruct** outData);

[DllImport("ReadFile.dll", CallingConvention = CallingConvention.Cdecl)]
unsafe private static extern void API_Free(DataStruct** handle);

unsafe static int ReadFile(string filename, out byte[] buffer)
{
    DataStruct* outData;
    int result = API_ReadFile(filename, &outData);
    buffer = new byte[outData->len];
    Marshal.Copy((IntPtr)outData->data, buffer, 0, outData->len);
    API_Free(&outData);
    return result;
}

static void Main(string[] args)
{
    byte[] buffer;
    ReadFile("test.txt", out buffer);
    foreach (byte ch in buffer)
    {
        Console.Write("{0} ", ch);
    }
    Console.Write("\n");
}
```


Whenever you can just use byte[] arrays for marshalling, otherwise you run into all sorts of troubles such as:
The C code:

```
typedef struct s_parameterStuct
{
    int count;
    char name[ 128 ];
} parameterStruct;
```

And the c# code:

```
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public class parameterStuct
{
    public int count;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
    public char[] name;
}
```

But since char is 2 bytes in c# the SizeConst for name should be 64 (becasue 64 of these occupy 128 bytes).


Further links:
http://stackoverflow.com/questions/17620396/tutorial-needed-on-invoking-unmanaged-dll-from-c-net





