using System.Diagnostics;

namespace BraidKit.Core;

public class BraidGame(Process _process, ProcessMemoryHandler _processMemoryHandler) : IDisposable
{
    public static BraidGame? GetFromOtherProcess()
    {
        var process = Process.GetProcessesByName("braid").FirstOrDefault();
        var braidGame = process != null ? new BraidGame(process, new(process.Id)) : null;
        return braidGame;
    }

    public static BraidGame GetFromCurrentProcess()
    {
        var process = Process.GetCurrentProcess();
        var braidGame = new BraidGame(process, new(process.Id));
        return braidGame;
    }

    public void Dispose()
    {
        _processMemoryHandler.Dispose();
        _process.Dispose();
    }

    public Process Process => _process;
    public bool IsSteamVersion => _process.Modules[0].ModuleMemorySize == 7663616;
    public bool IsRunning => !_process.HasExited;

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

    public GameValue<float> CameraPositionX { get; } = new(_processMemoryHandler, 0x5f6abc);
    public GameValue<float> CameraPositionY { get; } = new(_processMemoryHandler, 0x5f6ac0);
    public GameValue<int> IdealWidth { get; } = new(_processMemoryHandler, 0x5f6a90, 1280);
    public GameValue<int> IdealHeight { get; } = new(_processMemoryHandler, 0x5f6a94, 720);

    public float Zoom
    {
        get => IdealWidth.DefaultValue / (float)IdealWidth.Value;
        set
        {
            IdealWidth.Value = (int)Math.Round(IdealWidth.DefaultValue / value);
            IdealHeight.Value = (int)Math.Round(IdealHeight.DefaultValue / value);

            // TODO: Fix issues with the in-game menu caused by zooming
            const IntPtr _updatePostprocessFxAddr = 0x004f8960;
            _processMemoryHandler.CallFunction(_updatePostprocessFxAddr);
        }
    }

    private const IntPtr _initialPuzzlePieceAddr = 0x5f7584;
    private const IntPtr _worldPuzzlePieceOffset = 0x18c;
    private const IntPtr _individualPuzzlePieceOffset = 0x20;

    private PointerPath TimPointerPath { get; } = new(_processMemoryHandler, 0x5f6de8, 0x30, 0xc4, 0x8);
    public Entity GetTim() => new(_processMemoryHandler, TimPointerPath.GetAddress()!.Value);

    private PointerPath GreeterPointerPath { get; } = new(_processMemoryHandler, 0x5f6de8, 0x30, 0xac, 0x8);
    public GreeterEntity? GetDinosaurAkaGreeter() => GreeterPointerPath.GetAddress() is IntPtr addr ? new Entity(_processMemoryHandler, addr).AsGreeter() : null;

    private GameValue<double> TimRunSpeed { get; } = new(_processMemoryHandler, 0x5f6f08, 200);
    private GameValue<double> TimAirSpeed { get; } = new(_processMemoryHandler, 0x5f6f30, 200);
    private GameValue<double> TimClimbSpeed { get; } = new(_processMemoryHandler, 0x5f6f20, 173.3);
    public float TimSpeedMultiplier
    {
        get => (float)(TimRunSpeed.Value / TimRunSpeed.DefaultValue);
        set
        {
            TimRunSpeed.Value = TimRunSpeed.DefaultValue * value;
            TimAirSpeed.Value = TimAirSpeed.DefaultValue * value;
            TimClimbSpeed.Value = TimClimbSpeed.DefaultValue * value;
        }
    }

    private GameValue<double> TimJumpSpeed { get; } = new(_processMemoryHandler, 0x5f6f28, 360);
    public float TimJumpMultiplier
    {
        get => (float)(TimJumpSpeed.Value / TimJumpSpeed.DefaultValue);
        set => TimJumpSpeed.Value = TimJumpSpeed.DefaultValue * value;
    }

    public GameValue<int> TimLevelState { get; } = new(_processMemoryHandler, 0x5f93c0); // Level transition type
    public bool TimEnterDoor => TimLevelState == 1;
    public bool TimEnterLevel => TimLevelState == 4;
    public bool TimTouchedFlagpole => GetDinosaurAkaGreeter()?.IsGreeterWalking ?? false;

    public GameValue<int> TimWorld { get; } = new(_processMemoryHandler, 0x5f718c);
    public GameValue<int> TimLevel { get; } = new(_processMemoryHandler, 0x5f7190);

    public GameValue<int> FrameCount { get; } = new(_processMemoryHandler, 0x5f94b0); // Frame index

    public GameValue<bool> DrawDebugInfo { get; } = new(_processMemoryHandler, 0x5f6dcf);

    private GameValue<byte> SleepPaddingHasFocusCompareValue { get; } = new(_processMemoryHandler, 0x4b51ec, 0x0);
    private const byte _invalidBool = 0x2;
    public bool FullSpeedInBackground
    {
        get => SleepPaddingHasFocusCompareValue.Value == _invalidBool;
        set => SleepPaddingHasFocusCompareValue.Value = value ? _invalidBool : SleepPaddingHasFocusCompareValue.DefaultValue;
    }

    private PointerPath DisplaySystemPointerPath { get; } = new(_processMemoryHandler, 0xb2989c, 0x4);
    public DisplaySystem DisplaySystem => new(_processMemoryHandler, DisplaySystemPointerPath.GetAddress()!.Value);

    public void AddWatermark() => _processMemoryHandler.Write(0x00507bda, 0x00579e10);

    public List<Entity> GetEntities()
    {
        const int entityManagerPointerAddr = 0x005f6de8;
        var entityManagerAddr = _processMemoryHandler.Read<int>(entityManagerPointerAddr);
        var entityAddrs = new AutoArray<int>(_processMemoryHandler, entityManagerAddr + 0xc).GetAllItems();
        var entities = entityAddrs.Select(x => new Entity(_processMemoryHandler, x)).ToList();
        return entities;
    }

    public void ResetPieces()
    {
        // Note: Pieces in current level don't reset properly, but maybe that's a good thing for IL speedrunning
        for (int world = 0; world < 5; world++)
            for (int piece = 0; piece < 12; piece++)
                // TODO: Write<byte>?
                _processMemoryHandler.Write(_initialPuzzlePieceAddr + _worldPuzzlePieceOffset * world + _individualPuzzlePieceOffset * piece, 0);
    }
}
