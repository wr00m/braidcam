namespace BraidCam;

internal static class Program
{
    static void Main(string[] args)
    {
        if (!Options.TryParse(args, out var options)) { Console.WriteLine("Usage: braidcam [lock | unlock] [x y]"); return; }
        if (!BraidGame.TryGetRunningInstance(out var braidGame)) { Console.WriteLine("Braid is not running"); return; }
        if (!braidGame.IsSteamVersion) { Console.WriteLine("Only Steam version of Braid is supported"); return; }

        braidGame.CamLockX = braidGame.CamLockY = options.LockCam ?? !braidGame.CamLockX;
        if (options.SetCamPosX != null) { braidGame.CamPosX = options.SetCamPosX.Value; }
        if (options.SetCamPosY != null) { braidGame.CamPosY = options.SetCamPosY.Value; }

        Console.WriteLine($"Camera is {(braidGame.CamLockX ? "locked" : "unlocked")} at x={braidGame.CamPosX:0} y={braidGame.CamPosY:0}");
    }
}
