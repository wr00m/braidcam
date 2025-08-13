using System.CommandLine;

namespace BraidKit;

internal static class Commands
{
    internal static RootCommand RootCommand =>
        new("braidkit")
        {
            CameraLockCommand,
            CameraZoomCommand,
            TimPositionCommand,
            TimVelocityCommand,
            TimSpeedMultiplierCommand,
            TimJumpMultiplierCommand,
            EntityFlagsCommand,
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
            var tim = braidGame.GetTim();
            if (parseResult.GetValue<float?>("x") is float x)
            {
                tim.PositionX = relative ? tim.PositionX + x : x;
                tim.DetachFromGround();
            }
            if (parseResult.GetValue<float?>("y") is float y)
            {
                tim.PositionY = relative ? tim.PositionY + y : y;
                tim.DetachFromGround();
            }
            OutputTimPosition(braidGame);
        }, watermark: true);

    private static Command TimVelocityCommand =>
        new Command("tim-velocity", "Sets Tim's velocity")
        {
            new Argument<float?>("x") { Description = "Tim's x velocity" },
            new Argument<float?>("y") { Description = "Tim's y velocity" },
            new Option<bool>("--relative", "-r") { Description = "Relative to current velocity" },
        }.SetBraidGameAction((braidGame, parseResult) =>
        {
            var relative = parseResult.GetValue<bool>("--relative");
            var tim = braidGame.GetTim();
            if (parseResult.GetValue<float?>("x") is float x)
            {
                tim.VelocityX = relative ? tim.VelocityX + x : x;
                tim.DetachFromGround();
            }
            if (parseResult.GetValue<float?>("y") is float y)
            {
                tim.VelocityY = relative ? tim.VelocityY + y : y;
                tim.DetachFromGround();
            }
            OutputTimVelocity(braidGame);
        }, watermark: true);

    private static Command TimSpeedMultiplierCommand =>
        new Command("tim-speed", "Sets Tim's movement speed multiplier")
        {
            new Argument<float?>("multiplier") { Description = "Tim's speed multiplier" },
            TimSpeedMultiplierResetCommand,
        }.SetBraidGameAction((braidGame, parseResult) =>
        {
            if (parseResult.GetValue<float?>("multiplier") is float multiplier)
                braidGame.TimSpeedMultiplier = multiplier;
            OutputTimSpeedMultiplier(braidGame);
        }, watermark: true);

    private static Command TimSpeedMultiplierResetCommand =>
        new Command("reset", "Resets Tim's movement speed multiplier")
        .SetBraidGameAction((braidGame, parseResult) =>
        {
            braidGame.TimSpeedMultiplier = 1;
            OutputTimSpeedMultiplier(braidGame);
        });

    private static Command TimJumpMultiplierCommand =>
        new Command("tim-jump", "Sets Tim's jump speed multiplier")
        {
            new Argument<float?>("multiplier") { Description = "Tim's jump multiplier" },
            TimJumpMultiplierResetCommand
        }.SetBraidGameAction((braidGame, parseResult) =>
        {
            if (parseResult.GetValue<float?>("multiplier") is float multiplier)
                braidGame.TimJumpMultiplier = multiplier;
            OutputTimJumpMultiplier(braidGame);
        }, watermark: true);

    private static Command TimJumpMultiplierResetCommand =>
        new Command("reset", "Resets Tim's jump speed multiplier")
        .SetBraidGameAction((braidGame, parseResult) =>
        {
            braidGame.TimJumpMultiplier = 1;
            OutputTimJumpMultiplier(braidGame);
        });

    private static Command ToggleFullSpeedInBackgroundCommand =>
        new Command("bg-full-speed", "Toggles game running at full speed in background")
        .SetBraidGameAction((braidGame, parseResult) =>
        {
            braidGame.FullSpeedInBackground = !braidGame.FullSpeedInBackground;
            OutputFullSpeedInBackground(braidGame);
        }, watermark: true);

    private static Command ToggleDebugInfoCommand =>
        new Command("debug-info", "Toggles in-game debug info")
        .SetBraidGameAction((braidGame, parseResult) =>
        {
            braidGame.DrawDebugInfo = !braidGame.DrawDebugInfo;
            OutputShowDebugInfo(braidGame);
        });

    private enum SelectEntity { All, Tim, ClosestToTim }
    private enum OnOff { Off, On }
    private static Command EntityFlagsCommand =>
        new Command("entity-flag", "Sets behavior flags for game entities")
        {
            // TODO: Reactivate ClosestToTim when it's working properly
            new Argument<SelectEntity>("entity").AcceptOnlyFromAmong(SelectEntity.All.ToString(), SelectEntity.Tim.ToString()),
            new Argument<EntityFlags>("flag"),
            new Argument<OnOff>("value"),
        }
        .SetBraidGameAction((braidGame, parseResult) =>
        {
            var entity = parseResult.GetRequiredValue<SelectEntity>("entity");
            var entityFlag = parseResult.GetRequiredValue<EntityFlags>("flag");
            var value = parseResult.GetRequiredValue<OnOff>("value");

            var entities = braidGame.GetEntities();
            var tim = entities.Single(x => x.EntityType == EntityType.Tim);

            entities = value switch
            {
                OnOff.On => entities.Where(x => !x.EntityFlags.HasFlag(entityFlag)).ToList(),
                OnOff.Off => entities.Where(x => x.EntityFlags.HasFlag(entityFlag)).ToList(),
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
            };

            entities = entity switch
            {
                SelectEntity.All => entities,
                SelectEntity.Tim => [.. entities.Where(x => x == tim)],
                SelectEntity.ClosestToTim => [.. entities.Where(x => x != tim).OrderBy(x => x.GetDistanceSquared(tim)).Take(1)],
                _ => throw new ArgumentOutOfRangeException(nameof(entity), entity, null),
            };

            switch (value)
            {
                case OnOff.On:
                    entities.ForEach(x => x.EntityFlags |= entityFlag);
                    break;
                case OnOff.Off:
                    entities.ForEach(x => x.EntityFlags &= ~entityFlag);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }

            Console.WriteLine($"Flag turned {value.ToString().ToLower()} for {entities.Count} {(entities.Count == 1 ? "entity" : "entities")}");
        }, watermark: true);

    private static Command SetBraidGameAction(this Command cmd, Action<BraidGame, ParseResult> action, bool watermark = false)
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

            if (watermark)
                braidGame.AddWatermark();

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
    private static void OutputTimPosition(BraidGame braidGame) => Console.WriteLine($"Tim's position is x={braidGame.GetTim().PositionX:0.##} y={braidGame.GetTim().PositionY:0.##}");
    private static void OutputTimVelocity(BraidGame braidGame) => Console.WriteLine($"Tim's velocity is x={braidGame.GetTim().VelocityX:0.##} y={braidGame.GetTim().VelocityY:0.##}");
    private static void OutputTimSpeedMultiplier(BraidGame braidGame) => Console.WriteLine($"Tim's speed multiplier is {braidGame.TimSpeedMultiplier:0.##}");
    private static void OutputTimJumpMultiplier(BraidGame braidGame) => Console.WriteLine($"Tim's jump multiplier is {braidGame.TimJumpMultiplier:0.##}");
    private static void OutputFullSpeedInBackground(BraidGame braidGame) => Console.WriteLine($"Full game speed in background is {(braidGame.FullSpeedInBackground ? "on" : "off")}");
    private static void OutputShowDebugInfo(BraidGame braidGame) => Console.WriteLine($"Debug info is {(braidGame.DrawDebugInfo ? "on" : "off")}");
}
