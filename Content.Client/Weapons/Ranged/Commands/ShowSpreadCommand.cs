using Content.Client.Weapons.Ranged.Systems;
using Robust.Shared.Console;
using Robust.Shared.Reflection;
using System.Linq;
using static Content.Client.Weapons.Ranged.Systems.GunSystem;

namespace Content.Client.Weapons.Ranged;

public sealed class ShowSpreadCommand : IConsoleCommand
{
    [Dependency] private readonly IReflectionManager _wehavereflectionathome = default!;

    public string Command => "showgunspread";
    public string Description => $"Shows gun spread overlay for debugging";
    public string Help => $"{Command} off/partial/full";

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length <= 1)
            return CompletionResult.FromOptions(["off", "partial", "full"]);
        return CompletionResult.Empty;
    }

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var system = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<GunSystem>();

        if (args.Length != 1 ||
          !_wehavereflectionathome.TryParseEnumReference($"enum.{nameof(GunSpreadOverlayEnum)}.{args[0]}", out var e, false))
        {
            shell.WriteLine(Help);
            return;
        }
        GunSpreadOverlayEnum option = (GunSpreadOverlayEnum) e;
        if (system.SpreadOverlay == option)
        {
            shell.WriteLine($"Spread overlay already set to \"{system.SpreadOverlay}\".");
        }
        else {
            system.SpreadOverlay = option;
            shell.WriteLine($"Set spread overlay to \"{system.SpreadOverlay}\".");
        }
    }
}

public sealed class ShowFullSpreadCommand : IConsoleCommand
{
    public string Command => "showgunspreadfull";
    public string Description => $"Shows gun spread overlay for debugging";
    public string Help => $"{Command}";
    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var system = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<GunSystem>();
        if (system.SpreadOverlay == GunSystem.GunSpreadOverlayEnum.Full)
            system.SpreadOverlay = GunSystem.GunSpreadOverlayEnum.Off;
        else
            system.SpreadOverlay = GunSystem.GunSpreadOverlayEnum.Full;

        shell.WriteLine($"Set spread overlay to \"{system.SpreadOverlay}\".");
    }
}
