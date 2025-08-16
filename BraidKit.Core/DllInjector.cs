using InjectDotnet;
using System.Diagnostics;

namespace BraidKit.Core;

public static class DllInjector
{
    public static bool ToggleHitboxes(this Process process) => process.InjectBraidKitDllIntoGame("ToggleHitboxes") != 0;

    private static int InjectBraidKitDllIntoGame(this Process process, string method)
    {
        return process.Inject(
            runtimeconfig: "BraidKit.runtimeconfig.json",
            dllToInject: "BraidKit.Inject.dll",
            asssemblyQualifiedTypeName: "BraidKit.Inject.Bootstrapper, BraidKit.Inject, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
            method: method,
            argument: "",
            waitForReturn: true)!.Value;
    }
}
