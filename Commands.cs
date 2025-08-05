using System.CommandLine;

namespace BraidCam;

internal static class Commands
{
    internal static RootCommand RootCommand(BraidGame braidGame)
    {
        return new("braidcam")
        {
            CameraLockCommand(braidGame),
            CameraUnlockCommand(braidGame),
            CameraZoomCommand(braidGame),
            TimPositionCommand(braidGame),
            TimVelocityCommand(braidGame),
            ToggleFullSpeedInBackgroundCommand(braidGame),
            ToggleDebugInfoCommand(braidGame),
        };
    }

    private static Command CameraLockCommand(BraidGame braidGame)
    {
        var cmd = new Command("camera-lock", "Locks camera position")
        {
            new Argument<float?>("x") { Description = "Camera x position" },
            new Argument<float?>("y") { Description = "Camera y position" },
            new Option<bool>("--relative", "-r") { Description = "Relative to current position" },
            CameraLockToggleCommand(braidGame),
        };
        cmd.SetAction(parseResult =>
        {
            var relative = parseResult.GetValue<bool>("--relative");
            braidGame.CameraLock = true;
            if (parseResult.GetValue<float?>("x") is float x)
                braidGame.CameraPositionX = relative ? braidGame.CameraPositionX + x : x;
            if (parseResult.GetValue<float?>("y") is float y)
                braidGame.CameraPositionY = relative ? braidGame.CameraPositionY + y : y;
            OutputCameraPosition(braidGame);
        });
        return cmd;
    }

    private static Command CameraLockToggleCommand(BraidGame braidGame)
    {
        var cmd = new Command("toggle", "Toggles camera position locked/unlocked");
        cmd.SetAction(parseResult =>
        {
            braidGame.CameraLock = !braidGame.CameraLock;
            OutputCameraPosition(braidGame);
        });
        return cmd;
    }

    private static Command CameraUnlockCommand(BraidGame braidGame)
    {
        var cmd = new Command("camera-unlock", "Unlocks camera position");
        cmd.SetAction(parseResult =>
        {
            braidGame.CameraLock = false;
            OutputCameraPosition(braidGame);
        });
        return cmd;
    }

    private static Command CameraZoomCommand(BraidGame braidGame)
    {
        var cmd = new Command("camera-zoom", "Sets camera zoom (experimental, may cause issues with the GUI)")
        {
            new Argument<float?>("zoom") { Description="Default is 1, <1 zooms out, >1 zooms in" },
            CameraZoomResetCommand(braidGame),
        };
        cmd.SetAction(parseResult =>
        {
            if (parseResult.GetValue<float?>("zoom") is float zoom)
                braidGame.Zoom = zoom;
            OutputCameraZoom(braidGame);
        });
        return cmd;
    }

    private static Command CameraZoomResetCommand(BraidGame braidGame)
    {
        var cmd = new Command("reset", "Resets camera zoom to default value");
        cmd.SetAction(parseResult =>
        {
            const float defaultZoom = 1;
            braidGame.Zoom = defaultZoom;
            OutputCameraZoom(braidGame);
        });
        return cmd;
    }

    private static Command TimPositionCommand(BraidGame braidGame)
    {
        var cmd = new Command("tim-position", "Sets Tim's position")
        {
            new Argument<float?>("x") { Description = "Tim's x position" },
            new Argument<float?>("y") { Description = "Tim's y position" },
            new Option<bool>("--relative", "-r") { Description = "Relative to current position" },
        };
        cmd.SetAction(parseResult =>
        {
            var relative = parseResult.GetValue<bool>("--relative");
            if (parseResult.GetValue<float?>("x") is float x)
                braidGame.TimPositionX = relative ? braidGame.TimPositionX + x : x;
            if (parseResult.GetValue<float?>("y") is float y)
                braidGame.TimPositionY = relative ? braidGame.TimPositionY + y : y;
            OutputTimPosition(braidGame);
        });
        return cmd;
    }

    private static Command TimVelocityCommand(BraidGame braidGame)
    {
        var cmd = new Command("tim-velocity", "Sets Tim's velocity")
        {
            new Argument<float?>("x") { Description = "Tim's x velocity" },
            new Argument<float?>("y") { Description = "Tim's y velocity" },
            new Option<bool>("--relative", "-r") { Description = "Relative to current velocity" },
        };
        cmd.SetAction(parseResult =>
        {
            var relative = parseResult.GetValue<bool>("--relative");
            if (parseResult.GetValue<float?>("x") is float x)
                braidGame.TimVelocityX = relative ? braidGame.TimVelocityX + x : x;
            if (parseResult.GetValue<float?>("y") is float y)
                braidGame.TimVelocityY = relative ? braidGame.TimVelocityY + y : y;
            OutputTimVelocity(braidGame);
        });
        return cmd;
    }

    private static Command ToggleFullSpeedInBackgroundCommand(BraidGame braidGame)
    {
        var cmd = new Command("bg-full-speed", "Toggles game running at full speed in background");
        cmd.SetAction(parseResult =>
        {
            braidGame.FullSpeedInBackground = !braidGame.FullSpeedInBackground;
            OutputFullSpeedInBackground(braidGame);
        });
        return cmd;
    }

    private static Command ToggleDebugInfoCommand(BraidGame braidGame)
    {
        var cmd = new Command("debug-info", "Toggles in-game debug info");
        cmd.SetAction(parseResult =>
        {
            braidGame.DrawDebugInfo = !braidGame.DrawDebugInfo;
            OutputShowDebugInfo(braidGame);
        });
        return cmd;
    }

    private static void OutputCameraPosition(BraidGame braidGame) => Console.WriteLine($"Camera is {(braidGame.CameraLock ? "locked" : "unlocked")} at x={braidGame.CameraPositionX:0.##} y={braidGame.CameraPositionY:0.##}");
    private static void OutputCameraZoom(BraidGame braidGame) => Console.WriteLine($"Camera zoom is {braidGame.Zoom:0.##}");
    private static void OutputTimPosition(BraidGame braidGame) => Console.WriteLine($"Tim's position is x={braidGame.TimPositionX:0.##} y={braidGame.TimPositionY:0.##}");
    private static void OutputTimVelocity(BraidGame braidGame) => Console.WriteLine($"Tim's velocity is x={braidGame.TimVelocityX:0.##} y={braidGame.TimVelocityY:0.##}");
    private static void OutputFullSpeedInBackground(BraidGame braidGame) => Console.WriteLine($"Full game speed in background is {(braidGame.FullSpeedInBackground ? "on" : "off")}");
    private static void OutputShowDebugInfo(BraidGame braidGame) => Console.WriteLine($"Debug info is {(braidGame.DrawDebugInfo ? "on" : "off")}");
}
