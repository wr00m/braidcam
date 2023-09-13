namespace BraidCam;

using System.Runtime.InteropServices;

class ProcessMemoryHandler : IDisposable
{
    private IntPtr _hProc;
    private IntPtr _baseAddress;

    public ProcessMemoryHandler(int processId, IntPtr baseAddress)
    {
        _hProc = OpenProcess(ProcessAccessFlags.All, false, processId);
        _baseAddress = baseAddress;
    }

    public void Dispose()
    {
        if (_hProc != default) { CloseHandle(_hProc); }
    }

    public float ReadFloat(IntPtr addr) => BitConverter.ToSingle(ReadBytes(addr, sizeof(float)), 0);
    public void WriteFloat(IntPtr addr, float val) => WriteBytes(addr, BitConverter.GetBytes(val));
    public byte[] ReadBytes(IntPtr addr, int count) { var buff = new byte[count]; ReadProcessMemory(_hProc, _baseAddress + addr, buff, count, out _); return buff; }
    public void WriteBytes(IntPtr addr, byte[] bytes) => WriteProcessMemory(_hProc, _baseAddress + addr, bytes, (uint)bytes.Length, out var _);

    [DllImport("kernel32.dll")] static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);
    [DllImport("kernel32.dll", SetLastError = true)] static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);
    [DllImport("kernel32.dll", SetLastError = true)] static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);
    [DllImport("kernel32.dll")] static extern int CloseHandle(IntPtr hProcess);
    enum ProcessAccessFlags : uint { All = 0x001F0FFF }
}
