using BraidKit.Core;
using System.CommandLine;

namespace BraidKit.Commands;

internal static class CommandHelper
{
    internal static Command SetBraidGameAction(this Command cmd, Action<BraidGame, ParseResult> action, bool watermark = false)
    {
        cmd.SetAction(parseResult =>
        {
            using var braidGame = BraidGame.GetFromOtherProcess();

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
}
