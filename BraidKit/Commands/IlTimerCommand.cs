using System.CommandLine;

namespace BraidKit.Commands;

internal static partial class Commands
{
    private static Command IlTimerCommand =>
        new Command("il-timer", "Prints level complete times (flag levels not supported)")
        {
            new Option<bool>("--reset-pieces", "-rp") { Description = "Reset ALL pieces on door entry" },
        }
        .SetBraidGameAction((braidGame, parseResult) =>
        {
            var resetPieces = parseResult.GetValue<bool>("--reset-pieces");
            Console.WriteLine("IL timing enabled. Press Ctrl+C to exit.");

            var oldState = braidGame.TimLevelState.Value;
            var initialFrame = braidGame.FrameCount.Value;

            // TODO: Maybe try reducing this loop's CPU load by using something like SpinWait(-1).
            // Thread.Sleep(1) probably has insufficient resolution on Windows.
            // Our best option is probably to use game event hooks instead of polling.
            while (true)
            {
                var currentState = braidGame.TimLevelState.Value;
                if (oldState == currentState)
                    continue;

                // TODO: Pause timer during puzzle screen
                // TODO: Stop timer when flagpole is reached

                if (braidGame.TimEnterDoor)
                {
                    var currentFrame = braidGame.FrameCount.Value;
                    var levelFrames = currentFrame - initialFrame;
                    var levelSeconds = levelFrames / 60.0;
                    Console.WriteLine($"\nLevel: {braidGame.TimWorld}-{braidGame.TimLevel}");
                    Console.WriteLine($"Time: {levelSeconds:0.00}");

                    if (resetPieces)
                        braidGame.ResetPieces();
                }

                if (braidGame.TimEnterLevel)
                    initialFrame = braidGame.FrameCount;

                oldState = currentState;
            }
        });
}