using HarmonyLib;
using System.Reflection;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace RacesTweaks
{
    public class RacesTweaksModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            InformationManager.DisplayMessage(
                new InformationMessage("RaceTweaks Mod loaded successfully."));
            
            Harmony harmony = new Harmony("RaceTweaks");
            harmony.PatchAll();
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
        }


    }
}
