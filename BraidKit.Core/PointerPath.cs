namespace BraidKit.Core;

public class PointerPath(ProcessMemoryHandler _processMemoryHandler, params IntPtr[] _addrOffsets)
{
    public IntPtr? GetAddress()
    {
        IntPtr addr = 0;
        foreach (var offset in _addrOffsets)
        {
            addr = _processMemoryHandler.Read<int>(addr + offset);

            // Stop evaluating pointer path if we got a null pointer
            if (addr == IntPtr.Zero)
                return null;
        }
        return addr;
    }
}
