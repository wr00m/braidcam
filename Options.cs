namespace BraidCam;

record Options
{
    public static bool TryParse(string[] args, out Options parsedArgs)
    {
        try
        {
            parsedArgs = args.Length is 2 or > 3 ? throw new ArgumentException() : new()
            {
                LockCam = args.Length == 0 ? null : args[0].ToLower() switch { "lock" => true, "unlock" => false, _ => throw new ArgumentOutOfRangeException() },
                SetCamPosX = args.Length == 3 ? float.Parse(args[1]) : null,
                SetCamPosY = args.Length == 3 ? float.Parse(args[2]) : null,
            };
            return true;
        }
        catch
        {
            parsedArgs = new();
            return false;
        }
    }

    public bool? LockCam { get; init; } // Toggle if null
    public float? SetCamPosX { get; init; }
    public float? SetCamPosY { get; init; }
}
