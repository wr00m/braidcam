namespace BraidKit.Core;

public static class ConsoleHelper
{
    public static void WriteWarning(string message)
    {
        using var _ = new TempConsoleColor(ConsoleColor.Yellow);
        Console.WriteLine(message);
    }

    public static void WriteError(string message)
    {
        using var _ = new TempConsoleColor(ConsoleColor.Red);
        Console.Error.WriteLine(message);
    }
}

public class TempConsoleColor : IDisposable
{
    private ConsoleColor _initialColor = Console.ForegroundColor;
    public TempConsoleColor(ConsoleColor color) => Console.ForegroundColor = color;
    public void Dispose() => Console.ForegroundColor = _initialColor;
}

public class TempCancelMessage : IDisposable
{
    private ConsoleCancelEventHandler _handler;
    public TempCancelMessage(string message)
    {
        _handler = new((_, _) => Console.WriteLine(message));
        Console.CancelKeyPress += _handler;
    }
    public void Dispose() => Console.CancelKeyPress -= _handler;
}
