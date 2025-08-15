namespace BraidKit;

internal class PointerPath(ProcessMemoryHandler _processMemoryHandler, params IntPtr[] _addrOffsets)
{
    public IntPtr GetAddress()
    {
        IntPtr addr = 0;
        foreach (var offset in _addrOffsets)
            addr = _processMemoryHandler.Read<int>(addr + offset);
        return addr;
    }
}
