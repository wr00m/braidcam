namespace BraidKit;

internal class Entity(ProcessMemoryHandler _processMemoryHandler, IntPtr _addr)
{
    public GameValue<EntityType> EntityType { get; } = new(_processMemoryHandler, _addr);
    public GameValue<EntityFlags> EntityFlags { get; } = new(_processMemoryHandler, _addr + 0x10);
    public GameValue<float> PositionX { get; } = new(_processMemoryHandler, _addr + 0x14);
    public GameValue<float> PositionY { get; } = new(_processMemoryHandler, _addr + 0x18);
    public GameValue<float> VelocityX { get; } = new(_processMemoryHandler, _addr + 0x1c);
    public GameValue<float> VelocityY { get; } = new(_processMemoryHandler, _addr + 0x20);
    public GameValue<float> Width { get; } = new(_processMemoryHandler, _addr + 0x24);
    public GameValue<float> Height { get; } = new(_processMemoryHandler, _addr + 0x28);
    private GameValue<int> SupportedByPortableId { get; } = new(_processMemoryHandler, _addr + 0x78);

    public void DetachFromGround() => SupportedByPortableId.Value = 0;

    public float GetDistanceSquared(Entity other)
    {
        var dx = other.PositionX.Value - PositionX.Value;
        var dy = other.PositionY.Value - PositionY.Value;
        return dx * dx + dy * dy;
    }
}

[Flags]
internal enum EntityFlags : uint
{
    // TODO: Identify more entity flags
    Hidden = 0x1,
    Immovable = 0x4,
    CollidablePlatformOrProjectile = 0x10,
    CollidableTerrain = 0x20,
    GreenGlow = 0x40,
    Climbable = 0x80,
    NoGravity = 0x4000,
    PurpleGlow = 0x10000,
    // TODO: 0x2000000u is apparently something
}

internal enum EntityType
{
    // Values are vtable memory addresses
    Tim = 0x579e60,
}
