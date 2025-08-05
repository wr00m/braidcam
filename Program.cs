using System.Globalization;

namespace BraidCam;

internal static class Program
{
    static async Task<int> Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        using var braidGame = BraidGame.GetRunningInstance();

        if (braidGame is null)
        {
            Console.WriteLine("Braid is not running");
            return ExitCodes.GameNotRunning;
        }

        if (!braidGame.IsSteamVersion)
        {
            Console.WriteLine("Only Steam version of Braid is supported");
            return ExitCodes.NotSteamVersion;
        }

        var rootCommand = Commands.RootCommand(braidGame);
        var parseResult = rootCommand.Parse(args);
        var exitCode = await parseResult.InvokeAsync();
        return exitCode;
    }

    private static class ExitCodes
    {
        internal const int GameNotRunning = 2;
        internal const int NotSteamVersion = 3;
    }
}
