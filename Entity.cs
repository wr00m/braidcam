namespace BraidKit;

internal class Entity
{
    private readonly ProcessMemoryHandler _processMemoryHandler;
    private readonly IntPtr _addr;
    private const int _entityFlagsOffset = 0x10;

    public Entity(ProcessMemoryHandler processMemoryHandler, IntPtr addr)
    {
        _processMemoryHandler = processMemoryHandler;
        _addr = addr;
    }

    public EntityType EntityType => (EntityType)_processMemoryHandler.ReadUInt(_addr);

    public EntityFlags EntityFlags
    {
        get => (EntityFlags)_processMemoryHandler.ReadUInt(_addr + _entityFlagsOffset);
        set => _processMemoryHandler.WriteUInt(_addr + _entityFlagsOffset, (uint)value);
    }

    public float GetDistanceSquared(Entity other)
    {
        var dx = other.PositionX - PositionX;
        var dy = other.PositionY - PositionY;
        return dx * dx + dy * dy;
    }

    public void DetachFromGround()
    {
        const int _supportedByPortableIdOffset = 0x78;
        _processMemoryHandler.WriteInt(_addr + _supportedByPortableIdOffset, 0);
    }

    private const int _positionXOffset = 0x14;
    public float PositionX
    {
        get => _processMemoryHandler.ReadFloat(_addr + _positionXOffset);
        set => _processMemoryHandler.WriteFloat(_addr + _positionXOffset, value);
    }

    private const int _positionYOffset = _positionXOffset + sizeof(int);
    public float PositionY
    {
        get => _processMemoryHandler.ReadFloat(_addr + _positionYOffset);
        set => _processMemoryHandler.WriteFloat(_addr + _positionYOffset, value);
    }

    private const int _velocityXOffset = 0x1c;
    public float VelocityX
    {
        get => _processMemoryHandler.ReadFloat(_addr + _velocityXOffset);
        set => _processMemoryHandler.WriteFloat(_addr + _velocityXOffset, value);
    }

    private const int _velocityYOffset = _velocityXOffset + sizeof(int);
    public float VelocityY
    {
        get => _processMemoryHandler.ReadFloat(_addr + _velocityYOffset);
        set => _processMemoryHandler.WriteFloat(_addr + _velocityYOffset, value);
    }
}

[Flags]
internal enum EntityFlags : uint
{
    // TODO: Identify more entity flags
    Hidden = 0x1,
    Immovable = 0x4,
    CollidablePlatform = 0x10,
    CollidableTerrain = 0x20,
    GreenGlow = 0x40,
    Climbable = 0x80,
    NoGravity = 0x4000,
    PurpleGlow = 0x10000,
}

internal enum EntityType
{
    // Values are vtable memory addresses
    Tim = 0x579e60,
}
