namespace BraidCam;

using System.Diagnostics;

class BraidGame : IDisposable
{
    public static bool TryGetRunningInstance(out BraidGame? braidGame)
    {
        var process = Process.GetProcessesByName("braid").FirstOrDefault();
        braidGame = process != null ? new(process) : null;
        return false;
    }

    private Process _process;
    private ProcessMemoryHandler _processMemoryHandler;
    public bool IsRunning => _process.HasExited == false;
    public bool IsSteamVersion => _process.Modules[0].ModuleMemorySize == 7663616;

    private BraidGame(Process process)
    {
        _process = process;
        _processMemoryHandler = new(process.Id, process.Modules[0].BaseAddress);
    }

    public void Dispose()
    {
        _processMemoryHandler.Dispose();
        _process.Dispose();
    }

    private static readonly byte[] _camEnabledBytes = new byte[] { 0xF3, 0x0F, 0x11 };
    private static readonly byte[] _camDisabledBytes = new byte[] { 0x90, 0x90, 0x90 };

    private const IntPtr _camUpdateXAddr = 0xA0367;
    public bool CamLockX
    {
        get => _processMemoryHandler!.ReadBytes(_camUpdateXAddr, _camDisabledBytes.Length).SequenceEqual(_camDisabledBytes);
        set => _processMemoryHandler!.WriteBytes(_camUpdateXAddr, value ? _camDisabledBytes : _camEnabledBytes);
    }

    private const IntPtr _camUpdateYAddr = 0xA036F;
    public bool CamLockY
    {
        get => _processMemoryHandler!.ReadBytes(_camUpdateYAddr, _camDisabledBytes.Length).SequenceEqual(_camDisabledBytes);
        set => _processMemoryHandler!.WriteBytes(_camUpdateYAddr, value ? _camDisabledBytes : _camEnabledBytes);
    }

    private const IntPtr camPosXAddr = 0x1F6ABC;
    public float CamPosX { get => _processMemoryHandler!.ReadFloat(camPosXAddr); set => _processMemoryHandler!.WriteFloat(camPosXAddr, value); }

    private const IntPtr camPosYAddr = camPosXAddr + sizeof(float);
    public float CamPosY { get => _processMemoryHandler!.ReadFloat(camPosYAddr); set => _processMemoryHandler!.WriteFloat(camPosYAddr, value); }
}
