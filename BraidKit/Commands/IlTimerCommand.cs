using BraidKit.Core;
using System.CommandLine;
using System.Runtime.InteropServices;

namespace BraidKit.Commands;

internal static partial class Commands
{
    private static Command IlTimerCommand =>
        new Command("il-timer", "Prints level complete times (flag levels not supported)")
        {
            // TODO: Aliases should be single-letter
            new Option<int?>("--world", "-w") { Description = "Only use timer for this world" },
            new Option<int?>("--level", "-l") { Description = "Only use timer for this level" },
            new Option<bool>("--reset-pieces", "-rp") { Description = "Reset ALL pieces on door entry" },
            new Option<bool>("--high-precision", "-hp") { Description = "Increases system timer resolution" },
        }
        .SetBraidGameAction((braidGame, parseResult) =>
        {
            var world = parseResult.GetValue<int?>("--world");
            var level = parseResult.GetValue<int?>("--level");
            var resetPieces = parseResult.GetValue<bool>("--reset-pieces");
            var highPrecision = parseResult.GetValue<bool>("--high-precision");

            Console.WriteLine("IL timer enabled. Press Ctrl+C to exit.");
            using var cancelMessage = new TempCancelMessage("IL timer stopped"); // Shown when Ctrl+C is pressed

            using var highPrecisionTimer = highPrecision ? new HighPrecisionTimer(10) : null;
            var ilTimer = new IlTimer(braidGame, world, level, resetPieces);

            while (braidGame.IsRunning)
                SpinWait.SpinUntil(() => ilTimer.Tick(), 5);

            ConsoleHelper.WriteWarning("IL timer stopped because game was closed");
        });
}

internal class IlTimer
{
    private readonly BraidGame _braidGame;
    private readonly int? _onlyWorld;
    private readonly int? _onlyLevel;
    private readonly bool _resetPieces;
    private bool _stopped;
    private int _currentWorld;
    private int _currentLevel;
    private int _frameIndex;
    private int _levelFrameCount;
    private bool _hasMissedImportantFrames; // True if we missed frames at start/pause/unpause/stop

    public IlTimer(BraidGame braidGame, int? onlyWorld = null, int? onlyLevel = null, bool resetPieces = false)
    {
        _braidGame = braidGame;
        _onlyWorld = onlyWorld;
        _onlyLevel = onlyLevel;
        _resetPieces = resetPieces;
        Restart();
    }

    private void Restart()
    {
        _currentWorld = _braidGame.TimWorld;
        _currentLevel = _braidGame.TimLevel;
        _stopped = (_onlyWorld != null && _currentWorld != _onlyWorld) || (_onlyLevel != null && _currentLevel != _onlyLevel);
        _frameIndex = _braidGame.FrameCount;
        _levelFrameCount = 0;
        _hasMissedImportantFrames = false;
    }

    private void Stop() => _stopped = true;

    /// <returns>True if a new frame was handled</returns>
    public bool Tick()
    {
        // Early exit if we have already polled this frame
        var prevFrameIndex = _frameIndex;
        _frameIndex = _braidGame.FrameCount;
        if (_frameIndex == prevFrameIndex)
            return false; // Keep polling

        var frameDelta = _frameIndex - prevFrameIndex;
        var hasMissedFrames = frameDelta > 1;

        // Restart timer if level has changed
        if (_braidGame.TimWorld != _currentWorld || _braidGame.TimLevel != _currentLevel)
        {
            Restart();
            _hasMissedImportantFrames |= hasMissedFrames;
            return true;
        }

        // Early exit if timer is stopped
        if (_stopped)
            return true;

        _levelFrameCount += frameDelta;

        // Stop timer if level is finished
        if (_braidGame.TimEnterDoor || _braidGame.TimTouchedFlagpole)
        {
            _hasMissedImportantFrames |= hasMissedFrames;
            Stop();

            const double fps = 60.0;
            var levelSeconds = _levelFrameCount / fps;

            Console.WriteLine($"\nLevel: {_currentWorld}-{_currentLevel}");
            Console.WriteLine($"Time: {levelSeconds:0.00}");

            if (_hasMissedImportantFrames)
                ConsoleHelper.WriteWarning("Retiming needed due to dropped frames");

            // TODO: Maybe this should be moved to Restart() so reset also happens when F1 is pressed?
            if (_resetPieces)
                _braidGame.ResetPieces();
        }

        return true;
    }
}

/// <summary>
/// Increases system timer resolution, allowing Thread.Sleep() and timers to be more accurate. Use with care.
/// </summary>
internal class HighPrecisionTimer : IDisposable
{
    private readonly uint _resolutionMs;
    private bool _disposed = false;

    public HighPrecisionTimer(uint resolutionMs = 1)
    {
        _resolutionMs = resolutionMs;
        _ = timeBeginPeriod(_resolutionMs);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _ = timeEndPeriod(_resolutionMs);
        GC.SuppressFinalize(this);
    }

    // Finalizer, in case someone forgets to call Dispose()
    ~HighPrecisionTimer() => Dispose();

    [DllImport("winmm.dll")] private static extern uint timeBeginPeriod(uint uMilliseconds);
    [DllImport("winmm.dll")] private static extern uint timeEndPeriod(uint uMilliseconds);
}