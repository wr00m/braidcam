using System.Diagnostics;
using System.Runtime.InteropServices;

bool lockCam, moveCam;
float posX, posY;
try
{
    ParseArgs(out lockCam, out moveCam, out posX, out posY);
}
catch
{
    Console.WriteLine("Usage: braidcam lock|unlock [x y]");
    return;
}

using var process = Process.GetProcessesByName("braid").FirstOrDefault();
if (process == null)
{
    Console.WriteLine("Braid is not running");
    return;
}

using ProcessModule module = process.Modules[0];
if (module.ModuleMemorySize != 7663616)
{
    Console.WriteLine("Only Steam version of Braid is supported");
    return;
}

var hProc = OpenProcess(ProcessAccessFlags.All, false, process.Id);
try
{
    const IntPtr camUpdateXAddr = 0xA0367;
    const IntPtr camUpdateYAddr = 0xA036F;
    var camUpdateOpcodes = lockCam ? new byte[] { 0x90, 0x90, 0x90 } : new byte[] { 0xF3, 0x0F, 0x11 };
    WriteBytes(camUpdateXAddr, camUpdateOpcodes);
    WriteBytes(camUpdateYAddr, camUpdateOpcodes);

    const IntPtr camPosXAddr = 0x1F6ABC;
    const IntPtr camPosYAddr = camPosXAddr + sizeof(float);
    if (moveCam)
    {
        WriteFloat(camPosXAddr, posX);
        WriteFloat(camPosYAddr, posY);
    }

    Console.WriteLine($"Camera is {(lockCam ? "locked" : "unlocked")} at x={ReadFloat(camPosXAddr):0} y={ReadFloat(camPosYAddr):0}");
}
finally
{
    CloseHandle(hProc);
}

void ParseArgs(out bool lockCam, out bool moveCam, out float posX, out float posY)
{
    if (args.Length is not 1 and not 3) throw new ArgumentOutOfRangeException();
    lockCam = args[0].ToLower() switch { "lock" => true, "unlock" => false, _ => throw new ArgumentOutOfRangeException() };
    moveCam = args.Length > 1;
    posX = moveCam ? float.Parse(args[1]) : default;
    posY = moveCam ? float.Parse(args[2]) : default;
}

float ReadFloat(IntPtr addr) => BitConverter.ToSingle(ReadBytes(addr, sizeof(float)), 0);
void WriteFloat(IntPtr addr, float val) => WriteBytes(addr, BitConverter.GetBytes(val));
byte[] ReadBytes(IntPtr addr, int count) { var buff = new byte[count]; ReadProcessMemory(hProc, module.BaseAddress + addr, buff, count, out _); return buff; }
void WriteBytes(IntPtr addr, byte[] bytes) => WriteProcessMemory(hProc, module.BaseAddress + addr, bytes, (uint)bytes.Length, out var _);
[DllImport("kernel32.dll")] static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);
[DllImport("kernel32.dll", SetLastError = true)] static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);
[DllImport("kernel32.dll", SetLastError = true)] static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);
[DllImport("kernel32.dll")] static extern int CloseHandle(IntPtr hProcess);
enum ProcessAccessFlags : uint { All = 0x001F0FFF }
