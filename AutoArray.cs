namespace BraidKit;

internal class AutoArray<T>(ProcessMemoryHandler _processMemoryHandler, IntPtr _addr) where T : unmanaged
{
    public int GetItemCount() => _processMemoryHandler.Read<int>(_addr);
    private IntPtr GetFirstItemAddr() => _processMemoryHandler.Read<int>(_addr + sizeof(int) * 2);
    private int GetItemSize() => ProcessMemoryHandler.GetBlittableSize<T>();

    public List<T> GetAllItems()
    {
        var result = new List<T>();
        var count = GetItemCount();
        var firstItemAddr = GetFirstItemAddr();
        var itemSize = GetItemSize();
        for (int i = 0; i < count; i++)
        {
            // TODO: Read all values at once, then split, for better performance
            var item = _processMemoryHandler.Read<T>(firstItemAddr + itemSize * i);
            result.Add(item);
        }
        return result;
    }
}
