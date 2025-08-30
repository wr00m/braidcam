namespace BraidKit.Core.MemoryAccess;

/// <summary>Represents a traditional C-style array that does not store its own length</summary>
internal class CArray(IntPtr _addr, int _stride)
{
    public IntPtr GetItemAddr(int index) => _addr + index * _stride;
}

/// <inheritdoc />
internal class CArray<T>(ProcessMemoryHandler _processMemoryHandler, IntPtr _addr)
    : CArray(_addr, ProcessMemoryHandler.GetBlittableSize<T>())
    where T : unmanaged
{
    public T GetItem(int index) => _processMemoryHandler.Read<T>(GetItemAddr(index));
}
