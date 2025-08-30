namespace BraidKit.Core.MemoryAccess;

/// <summary>Represents an implementation of std::list</summary>
internal class LinkedList<T>(ProcessMemoryHandler _processMemoryHandler, nint _addr) where T : unmanaged
{
    public const int StructSize = sizeof(int) * 3;
    public GameValue<int> ItemCount { get; } = new(_processMemoryHandler, _addr);
    private GameValue<nint> FirstNodeAddr { get; } = new(_processMemoryHandler, _addr + 0x4);
    private LinkedListNode? FirstNode => FirstNodeAddr != nint.Zero ? new(_processMemoryHandler, FirstNodeAddr) : null;

    public IEnumerable<T> GetItems()
    {
        for (var node = FirstNode; node != null; node = node.NextNode)
            yield return node.Data;
    }

    private class LinkedListNode(ProcessMemoryHandler _processMemoryHandler, nint _addr)
    {
        private GameValue<nint> NextNodeAddr { get; } = new(_processMemoryHandler, _addr);
        public LinkedListNode? NextNode => NextNodeAddr != nint.Zero ? new(_processMemoryHandler, NextNodeAddr) : null;
        public GameValue<T> Data { get; } = new(_processMemoryHandler, _addr + 0x8);
    }
}