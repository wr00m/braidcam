using System.CommandLine;

namespace BraidCam;

internal static class Commands
{
    internal static RootCommand RootCommand =>
        new("braidcam")
        {
            CameraLockCommand,
            CameraZoomCommand,
            TimPositionCommand,
            TimVelocityCommand,
            ToggleFullSpeedInBackgroundCommand,
            ToggleDebugInfoCommand,
        };

    private static Command CameraLockCommand =>
        new Command("camera-lock", "Locks camera position")
        {
            new Argument<float?>("x") { Description = "Camera x position" },
            new Argument<float?>("y") { Description = "Camera y position" },
            new Option<bool>("--relative", "-r") { Description = "Relative to current position" },
            CameraLockUnlockCommand,
            CameraLockToggleCommand,
        }
        .SetBraidGameAction((braidGame, parseResult) =>
        {
            var relative = parseResult.GetValue<bool>("--relative");
            braidGame.CameraLock = true;
            if (parseResult.GetValue<float?>("x") is float x)
                braidGame.CameraPositionX = relative ? braidGame.CameraPositionX + x : x;
            if (parseResult.GetValue<float?>("y") is float y)
                braidGame.CameraPositionY = relative ? braidGame.CameraPositionY + y : y;
            OutputCameraPosition(braidGame);
        });

    private static Command CameraLockToggleCommand =>
        new Command("toggle", "Toggles camera position locked/unlocked")
        .SetBraidGameAction((braidGame, parseResult) =>
        {
            braidGame.CameraLock = !braidGame.CameraLock;
            OutputCameraPosition(braidGame);
        });

    private static Command CameraLockUnlockCommand =>
        new Command("unlock", "Unlocks camera position")
        .SetBraidGameAction((braidGame, parseResult) =>
        {
            braidGame.CameraLock = false;
            OutputCameraPosition(braidGame);
        });

    private static Command CameraZoomCommand =>
        new Command("camera-zoom", "Sets camera zoom (experimental, may cause issues with the GUI)")
        {
            new Argument<float?>("zoom") { Description="Default is 1, <1 zooms out, >1 zooms in" },
            CameraZoomResetCommand,
        }.SetBraidGameAction((braidGame, parseResult) =>
        {
            if (parseResult.GetValue<float?>("zoom") is float zoom)
                braidGame.Zoom = zoom;
            OutputCameraZoom(braidGame);
        });

    private static Command CameraZoomResetCommand =>
        new Command("reset", "Resets camera zoom to default value")
        .SetBraidGameAction((braidGame, parseResult) =>
        {
            const float defaultZoom = 1;
            braidGame.Zoom = defaultZoom;
            OutputCameraZoom(braidGame);
        });

    private static Command TimPositionCommand =>
        new Command("tim-position", "Sets Tim's position")
        {
            new Argument<float?>("x") { Description = "Tim's x position" },
            new Argument<float?>("y") { Description = "Tim's y position" },
            new Option<bool>("--relative", "-r") { Description = "Relative to current position" },
        }.SetBraidGameAction((braidGame, parseResult) =>
        {
            var relative = parseResult.GetValue<bool>("--relative");
            if (parseResult.GetValue<float?>("x") is float x)
                braidGame.TimPositionX = relative ? braidGame.TimPositionX + x : x;
            if (parseResult.GetValue<float?>("y") is float y)
                braidGame.TimPositionY = relative ? braidGame.TimPositionY + y : y;
            OutputTimPosition(braidGame);
        });

    private static Command TimVelocityCommand =>
        new Command("tim-velocity", "Sets Tim's velocity")
        {
            new Argument<float?>("x") { Description = "Tim's x velocity" },
            new Argument<float?>("y") { Description = "Tim's y velocity" },
            new Option<bool>("--relative", "-r") { Description = "Relative to current velocity" },
        }.SetBraidGameAction((braidGame, parseResult) =>
        {
            var relative = parseResult.GetValue<bool>("--relative");
            if (parseResult.GetValue<float?>("x") is float x)
                braidGame.TimVelocityX = relative ? braidGame.TimVelocityX + x : x;
            if (parseResult.GetValue<float?>("y") is float y)
                braidGame.TimVelocityY = relative ? braidGame.TimVelocityY + y : y;
            OutputTimVelocity(braidGame);
        });

    private static Command ToggleFullSpeedInBackgroundCommand =>
        new Command("bg-full-speed", "Toggles game running at full speed in background")
        .SetBraidGameAction((braidGame, parseResult) =>
        {
            braidGame.FullSpeedInBackground = !braidGame.FullSpeedInBackground;
            OutputFullSpeedInBackground(braidGame);
        });

    private static Command ToggleDebugInfoCommand =>
        new Command("debug-info", "Toggles in-game debug info")
        .SetBraidGameAction((braidGame, parseResult) =>
        {
            braidGame.DrawDebugInfo = !braidGame.DrawDebugInfo;
            OutputShowDebugInfo(braidGame);
        });

    private static Command SetBraidGameAction(this Command cmd, Action<BraidGame, ParseResult> action)
    {
        cmd.SetAction(parseResult =>
        {
            using var braidGame = BraidGame.GetRunningInstance();

            if (braidGame is null)
            {
                OutputError("Braid is not running");
                return 1;
            }

            if (!braidGame.IsSteamVersion)
            {
                OutputError("Only Steam version of Braid is supported");
                return 1;
            }

            action(braidGame, parseResult);
            return 0;
        });
        return cmd;
    }

    private static void OutputError(string message)
    {
        var previous = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(message);
        Console.ForegroundColor = previous;
    }

    private static void OutputCameraPosition(BraidGame braidGame) => Console.WriteLine($"Camera is {(braidGame.CameraLock ? "locked" : "unlocked")} at x={braidGame.CameraPositionX:0.##} y={braidGame.CameraPositionY:0.##}");
    private static void OutputCameraZoom(BraidGame braidGame) => Console.WriteLine($"Camera zoom is {braidGame.Zoom:0.##}");
    private static void OutputTimPosition(BraidGame braidGame) => Console.WriteLine($"Tim's position is x={braidGame.TimPositionX:0.##} y={braidGame.TimPositionY:0.##}");
    private static void OutputTimVelocity(BraidGame braidGame) => Console.WriteLine($"Tim's velocity is x={braidGame.TimVelocityX:0.##} y={braidGame.TimVelocityY:0.##}");
    private static void OutputFullSpeedInBackground(BraidGame braidGame) => Console.WriteLine($"Full game speed in background is {(braidGame.FullSpeedInBackground ? "on" : "off")}");
    private static void OutputShowDebugInfo(BraidGame braidGame) => Console.WriteLine($"Debug info is {(braidGame.DrawDebugInfo ? "on" : "off")}");
}
