namespace BraidKit.Core;

public class GameValue<T>(ProcessMemoryHandler _processMemoryHandler, IntPtr _addr, T? _defaultValue = default) where T : unmanaged
{
    public T DefaultValue => _defaultValue ?? throw new Exception("Missing default value");
    public T Value { get => Get(); set => Set(value); }
    private T Get() => _processMemoryHandler.Read<T>(_addr);
    private void Set(T val) => _processMemoryHandler.Write(_addr, val);

    public override string? ToString() => Value.ToString();
    public static implicit operator T(GameValue<T> gameValue) => gameValue.Value;
}
