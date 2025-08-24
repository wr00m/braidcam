using BraidKit.Core;
using System.CommandLine;

namespace BraidKit.Commands;

internal static partial class Commands
{
    private static Command ToggleHitboxCommand =>
        new Command("show-hitbox", "Toggles in-game hitbox overlay (experimental)")
        .SetBraidGameAction((braidGame, parseResult) =>
        {
            var showHitboxes = braidGame.Process.ToggleHitboxes();
            Console.WriteLine($"Toggled hitbox overlay {(showHitboxes ? "on" : "off")}");
        });
}