using HarmonyLib;
using System;

namespace MeteorShowerChance.Patches
{
    [HarmonyPatch(typeof(TimeOfDay))]
    internal class MeteorPatch
    {
        [HarmonyPatch("DecideRandomDayEvents")]
        [HarmonyPostfix]
        public static void OverwriteMeteorChance(TimeOfDay __instance)
        {
            // This should only ever run server-side.
            if (!__instance.IsServer) return;
            // This shouldn't run on challenge files, either.
            if (MeteorChance.isChallengeFile) return;

            // This will be used more than once, so...
            bool isFixedRate = MeteorChance.Instance.chanceType.Value == "FixedRate";

            // Return early if the rateCap is zero (meteors are disabled).
            if (!isFixedRate && MeteorChance.Instance.rateCap.Value == 0f) return;

            System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed + 28);

            float rate = 0.7f;

            int daysWithoutMeteor = MeteorChance.daysWithoutMeteor;

            switch (MeteorChance.Instance.chanceType.Value)
            {
                case "FixedRate":
                    rate = MeteorChance.Instance.fixedRate.Value;
                    break;
                case "LinearRate":
                    // We add one here because the day counter starts at zero.
                    rate = MeteorChance.Instance.linearRate.Value * (daysWithoutMeteor + 1);
                    break;
                case "ExponentialRate":
                    // We subtract one here because otherwise 1% is technically added.
                    rate = (float) Math.Pow(MeteorChance.Instance.exponentialRate.Value, -daysWithoutMeteor) - 1;
                    break;
            }

            // Applying rate cap, when the rate type is not Fixed rate.
            if (!isFixedRate)
            {
                rate = Math.Min(rate, MeteorChance.Instance.rateCap.Value);
            }

            // Convert the percentage chance into an integer (1000 = 100%).
            int num = (int) Math.Ceiling(rate * 10);

            // Debug logs
            if (MeteorChance.debug)
            {
                MeteorChance.mls.LogInfo("Current chance today is " + (((float)num) / 10f) + "%");
                MeteorChance.mls.LogInfo("Day count: " + daysWithoutMeteor);
                MeteorChance.mls.LogInfo("Percentage full: " + rate);
            }


            if (random.Next(0, 1000) < num)
            {
                MeteorChance.daysWithoutMeteor = 0;
                __instance.meteorShowerAtTime = (float)random.Next(5, 80) / 100f;
            }
            else
            {
                MeteorChance.daysWithoutMeteor++;
                __instance.meteorShowerAtTime = -1f;
            }
        }
    }
}
