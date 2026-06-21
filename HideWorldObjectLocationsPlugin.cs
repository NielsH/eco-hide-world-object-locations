namespace Eco.Mods.HideWorldObjectLocations
{
    using System.Linq;
    using Eco.Core.Plugins;
    using Eco.Core.Plugins.Interfaces;
    using Eco.Core.Utils;
    using Eco.Shared.Utils;
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Systems.NewTooltip;
    using Eco.Shared.Localization;

    public class HideWorldObjectLocationsMod : IModInit
    {
        public static ModRegistration Register() => new()
        {
            ModName        = "HideWorldObjectLocations",
            ModDescription = "Removes the 'There is/are X <item> in the world' tooltip from all world object items.",
            ModDisplayName = "Hide World Object Locations"
        };
    }

    [Priority(PriorityAttribute.Low)]
    public class HideWorldObjectLocationsPlugin : Singleton<HideWorldObjectLocationsPlugin>, IModKitPlugin, IInitializablePlugin
    {
        const string TooltipPartName = "ExistingObjects";

        public void Initialize(TimedTask timer)
        {
            var tooltipManager = TooltipManagerServer.Obj;

            //Lock on the shared manager: plugin Initialize runs in parallel and other mods mutate these same
            //lists, so concurrent List.RemoveAll calls would corrupt them (the predicate can receive a null part).
            lock (tooltipManager)
            {
                //The ExistingObjects tooltip is registered under every type that derives from WorldObjectItem,
                //not just WorldObjectItem itself. Remove it from all of them.
                foreach (var (type, parts) in tooltipManager.TypeToParts)
                    parts.RemoveAll(p => p.Name == TooltipPartName);

                foreach (var (type, parts) in tooltipManager.ClientTypeToParts)
                    parts.RemoveAll(p => p.Name == TooltipPartName);

                //Clean up NameToPart for every type that had this part registered.
                var keysToRemove = tooltipManager.NameToPart.Keys.Where(k => k.FuncName == TooltipPartName).ToList();
                foreach (var key in keysToRemove)
                    tooltipManager.NameToPart.Remove(key);
            }
        }

        public string GetStatus()   => "World object location tooltips hidden";
        public string GetCategory() => Localizer.DoStr("Mods");
    }
}
