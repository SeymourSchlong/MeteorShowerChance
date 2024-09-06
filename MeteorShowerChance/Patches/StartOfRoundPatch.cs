using HarmonyLib;

namespace MeteorShowerChance.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        // Challenge moons should be unaffected by this chance adjustment.
        [HarmonyPatch("StartGame")]
        [HarmonyPostfix]
        public static void LeverPulled(StartOfRound __instance)
        {
            MeteorChance.isChallengeFile = __instance.isChallengeFile;
        }

        // Reset the meteor chance when the file is closed and another is opened.
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void StartSession()
        {
            if (MeteorChance.Instance.resetOnRestart.Value)
            {
                MeteorChance.daysWithoutMeteor = 0;
            }
        }

        // If the crew wipes, reset the values so they aren't inflated for the next run.
        [HarmonyPatch("ResetShip")]
        [HarmonyPostfix]
        public  static void ResetDayCount()
        {
            if (MeteorChance.Instance.resetOnRestart.Value)
            {
                MeteorChance.daysWithoutMeteor = 0;
            }
        }
    }
}
