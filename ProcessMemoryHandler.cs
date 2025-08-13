using System.Runtime.InteropServices;

namespace BraidKit;

internal class ProcessMemoryHandler : IDisposable
{
    private readonly IntPtr _hProc;

    public ProcessMemoryHandler(int processId)
    {
        _hProc = OpenProcess(ProcessAccessFlags.All, false, processId);
    }

    public void Dispose()
    {
        if (_hProc != default)
            CloseHandle(_hProc);
    }

    public bool ReadBool(IntPtr addr) => ReadByte(addr) != 0;
    public void WriteBool(IntPtr addr, bool val) => WriteByte(addr, val ? (byte)1 : (byte)0);
    public uint ReadUInt(IntPtr addr) => BitConverter.ToUInt32(ReadBytes(addr, sizeof(uint)), 0);
    public void WriteUInt(IntPtr addr, uint val) => WriteBytes(addr, BitConverter.GetBytes(val));
    public int ReadInt(IntPtr addr) => BitConverter.ToInt32(ReadBytes(addr, sizeof(int)), 0);
    public void WriteInt(IntPtr addr, int val) => WriteBytes(addr, BitConverter.GetBytes(val));
    public float ReadFloat(IntPtr addr) => BitConverter.ToSingle(ReadBytes(addr, sizeof(float)), 0);
    public void WriteFloat(IntPtr addr, float val) => WriteBytes(addr, BitConverter.GetBytes(val));
    public double ReadDouble(IntPtr addr) => BitConverter.ToDouble(ReadBytes(addr, sizeof(double)), 0);
    public void WriteDouble(IntPtr addr, double val) => WriteBytes(addr, BitConverter.GetBytes(val));
    public byte ReadByte(IntPtr addr) => ReadBytes(addr, 1)[0];
    public void WriteByte(IntPtr addr, byte val) => WriteBytes(addr, [val]);
    public byte[] ReadBytes(IntPtr addr, int count) { var buff = new byte[count]; ReadProcessMemory(_hProc, addr, buff, count, out _); return buff; }
    public void WriteBytes(IntPtr addr, byte[] bytes) => WriteProcessMemory(_hProc, addr, bytes, (uint)bytes.Length, out var _);
    public void CallFunction(IntPtr addr) => CreateRemoteThread(_hProc, IntPtr.Zero, 0, addr, IntPtr.Zero, 0, out _);

    public IntPtr GetAddressFromPointerPath(IntPtr[] pointerPath)
    {
        IntPtr addr = 0;
        foreach (var offset in pointerPath)
            addr = ReadInt(addr + offset);
        return addr;
    }

    public string ReadNullTerminatedString(IntPtr addr)
    {
        addr = ReadInt(addr);
        var result = "";
        for (var i = 0; true; i++)
        {
            var c = (char)ReadByte(addr + i);
            if (c == '\0')
                break;
            result += c;
        }
        return result;
    }

    [DllImport("kernel32.dll")] static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);
    [DllImport("kernel32.dll", SetLastError = true)] static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);
    [DllImport("kernel32.dll", SetLastError = true)] static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);
    [DllImport("kernel32.dll")] static extern int CloseHandle(IntPtr hProcess);
    [DllImport("kernel32.dll", SetLastError = true)] static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);
    private enum ProcessAccessFlags : uint { All = 0x001f0fff }
}
