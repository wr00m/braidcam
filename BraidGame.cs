using System.Diagnostics;

namespace BraidKit;

internal class BraidGame : IDisposable
{
    public static BraidGame? GetRunningInstance()
    {
        var process = Process.GetProcessesByName("braid").FirstOrDefault();
        var braidGame = process != null ? new BraidGame(process) : null;
        return braidGame;
    }

    private readonly Process _process;
    private readonly ProcessMemoryHandler _processMemoryHandler;

    private BraidGame(Process process)
    {
        _process = process;
        _processMemoryHandler = new(process.Id);
    }

    public void Dispose()
    {
        _processMemoryHandler.Dispose();
        _process.Dispose();
    }

    public bool IsSteamVersion => _process.Modules[0].ModuleMemorySize == 7663616;

    private static readonly byte[] _cameraEnabledBytes = [0xf3, 0x0f, 0x11];
    private static readonly byte[] _cameraDisabledBytes = [0x90, 0x90, 0x90];

    private const IntPtr _cameraUpdateXAddr = 0x4a0367;
    private bool CameraLockX
    {
        get => _processMemoryHandler.ReadBytes(_cameraUpdateXAddr, _cameraDisabledBytes.Length).SequenceEqual(_cameraDisabledBytes);
        set => _processMemoryHandler.WriteBytes(_cameraUpdateXAddr, value ? _cameraDisabledBytes : _cameraEnabledBytes);
    }

    private const IntPtr _cameraUpdateYAddr = 0x4a036f;
    private bool CameraLockY
    {
        get => _processMemoryHandler.ReadBytes(_cameraUpdateYAddr, _cameraDisabledBytes.Length).SequenceEqual(_cameraDisabledBytes);
        set => _processMemoryHandler.WriteBytes(_cameraUpdateYAddr, value ? _cameraDisabledBytes : _cameraEnabledBytes);
    }

    public bool CameraLock
    {
        get => CameraLockX || CameraLockY;
        set => CameraLockX = CameraLockY = value;
    }

    private const IntPtr _cameraPositionXAddr = 0x5f6abc;
    public float CameraPositionX
    {
        get => _processMemoryHandler.ReadFloat(_cameraPositionXAddr);
        set => _processMemoryHandler.WriteFloat(_cameraPositionXAddr, value);
    }

    private const IntPtr _cameraPositionYAddr = _cameraPositionXAddr + sizeof(float);
    public float CameraPositionY
    {
        get => _processMemoryHandler.ReadFloat(_cameraPositionYAddr);
        set => _processMemoryHandler.WriteFloat(_cameraPositionYAddr, value);
    }

    private const IntPtr _idealWidthAddr = 0x005f6a90;
    private int IdealWidth
    {
        get => _processMemoryHandler.ReadInt(_idealWidthAddr);
        set => _processMemoryHandler.WriteInt(_idealWidthAddr, value);
    }

    private const IntPtr _idealHeightAddr = _idealWidthAddr + sizeof(int);
    private int IdealHeight
    {
        get => _processMemoryHandler.ReadInt(_idealHeightAddr);
        set => _processMemoryHandler.WriteInt(_idealHeightAddr, value);
    }

    private const int _defaultIdealWidth = 1280;
    private const int _defaultIdealHeight = 720;
    private const IntPtr _updatePostprocessFxAddr = 0x004f8960;
    public float Zoom
    {
        get => _defaultIdealWidth / (float)IdealWidth;
        set
        {
            // TODO: Fix issues with the in-game menu caused by zooming
            IdealWidth = (int)Math.Round(_defaultIdealWidth / value);
            IdealHeight = (int)Math.Round(_defaultIdealHeight / value);
            _processMemoryHandler.CallFunction(_updatePostprocessFxAddr);
        }
    }

    private readonly IntPtr[] _timPointerPath = [0x400000 + 0x001f6de8, 0x30, 0xc4, 0x8];
    private IntPtr GetTimAddr(int offset) => _processMemoryHandler.GetAddressFromPointerPath(_timPointerPath) + offset;

    private const int _timPositionXOffset = 0x14;
    public float TimPositionX
    {
        get => _processMemoryHandler.ReadFloat(GetTimAddr(_timPositionXOffset));
        set => _processMemoryHandler.WriteFloat(GetTimAddr(_timPositionXOffset), value);
    }

    private const int _timPositionYOffset = _timPositionXOffset + sizeof(int);
    public float TimPositionY
    {
        get => _processMemoryHandler.ReadFloat(GetTimAddr(_timPositionYOffset));
        set => _processMemoryHandler.WriteFloat(GetTimAddr(_timPositionYOffset), value);
    }

    private const int _timVelocityXOffset = 0x1c;
    public float TimVelocityX
    {
        get => _processMemoryHandler.ReadFloat(GetTimAddr(_timVelocityXOffset));
        set => _processMemoryHandler.WriteFloat(GetTimAddr(_timVelocityXOffset), value);
    }

    private const int _timVelocityYOffset = _timVelocityXOffset + sizeof(int);
    public float TimVelocityY
    {
        get => _processMemoryHandler.ReadFloat(GetTimAddr(_timVelocityYOffset));
        set => _processMemoryHandler.WriteFloat(GetTimAddr(_timVelocityYOffset), value);
    }

    private const IntPtr _drawDebugInfoAddr = 0x005f6dcf;
    public bool DrawDebugInfo
    {
        get => _processMemoryHandler.ReadBool(_drawDebugInfoAddr);
        set => _processMemoryHandler.WriteBool(_drawDebugInfoAddr, value);
    }

    private const IntPtr _sleepPaddingHasFocusAddr = 0x004b51ec;
    private const byte _invalidBool = 0x2;
    public bool FullSpeedInBackground
    {
        get => _processMemoryHandler.ReadByte(_sleepPaddingHasFocusAddr) == _invalidBool;
        set => _processMemoryHandler.WriteByte(_sleepPaddingHasFocusAddr, value ? _invalidBool : (byte)0x0);
    }
}
