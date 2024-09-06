using BepInEx.Logging;
using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;

namespace MeteorShowerChance
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class MeteorChance : BaseUnityPlugin
    {
        private const string modGUID = "ironbean.MeteorShowerChance";
        private const string modName = "Meteor Shower Chance";
        private const string modVersion = "1.1.1";

        private readonly Harmony harmony = new Harmony(modGUID);

        internal static MeteorChance Instance;

        internal static ManualLogSource mls;

        // Plugin variables
        internal static int daysWithoutMeteor = 0;
        internal static bool isChallengeFile = false;
        internal static bool debug = false;

        // Config settings
        public ConfigEntry<bool> resetOnRestart;
        public ConfigEntry<float> rateCap;
        public ConfigEntry<string> chanceType;
        public ConfigEntry<float> fixedRate, linearRate, exponentialRate;

        void Awake()
        {
            if (Instance == null) Instance = this;

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo("Meteor Happens");

            // CONFIG

            resetOnRestart = Instance.Config.Bind<bool>(
                "Chances",
                "ResetOnRestart",
                true,
                new ConfigDescription(
                    "Whether or not Linear/Exponential rates retain the day count when reloading/a new save."
                )
            );

            rateCap = Instance.Config.Bind<float>(
                "Chances",
                "RateCap",
                15f,
                new ConfigDescription(
                    "The maximum chance for a meteor shower to occur. Set to 0% to disable meteor showers. If FixedRate is used, this value is ignored.",
                    new AcceptableValueRange<float>(0f, 100f)
                )
            );

            string[] typeValues = { "FixedRate", "LinearRate", "ExponentialRate" };

            chanceType = Instance.Config.Bind<string>(
                "Chances",
                "ChanceType",
                "FixedRate",
                new ConfigDescription(
                    "Determines which type of chance the meteor shower will use. ( FixedRate / LinearRate / ExponentialRate )",
                    new AcceptableValueList<string>(typeValues)
                )
            );

            fixedRate = Instance.Config.Bind<float>(
                "Chances",
                "FixedRate",
                1f,
                new ConfigDescription(
                    "When selected, the chance of a meteor shower occurring on a day will be this, in percentage.",
                    new AcceptableValueRange<float>(0f, 100f)
                )
            );

            linearRate = Instance.Config.Bind<float>(
                "Chances",
                "LinearRate",
                1f,
                new ConfigDescription(
                    "When selected, the chance of a meteor shower occurring on a day will increase by this amount daily, in percentage.",
                    new AcceptableValueRange<float>(0f, 100f)
                )
            );

            exponentialRate = Instance.Config.Bind<float>(
                "Chances",
                "ExponentialRate",
                0.96f,
                new ConfigDescription(
                    "When selected, the chance of a meteor shower occurring on a day will increase at a ramping curve. This value is used as the base for the exponent, multiplied by 100. Smaller value = more frequent. [ curve is: -1+Rate^(-DAYS_SINCE_METEORS) ]",
                    new AcceptableValueRange<float>(0.01f, 1f)
            )
            );


            string rateMsg = "Using a " + chanceType.Value + " of ";

            switch (chanceType.Value)
            {
                case "FixedRate":
                    rateMsg += fixedRate.Value + "%";
                    break;
                case "LinearRate":
                    rateMsg += linearRate.Value + "% added daily.";
                    break;
                case "ExponentialRate":
                    rateMsg += linearRate.Value + "^(-x)";
                    break;
                default:
                    rateMsg = "Unknown type";
                    break;
            }

            if (chanceType.Value != "Fixed Rate")
            {
                rateMsg += ", with a cap of " + rateCap.Value + "%";
                if (rateCap.Value == 0f)
                {
                    rateMsg = "Meteors are disabled.";
                }
            }

            mls.LogInfo(rateMsg);


            // PATCHES

            // Generic
            harmony.PatchAll(typeof(MeteorChance));
            // meteor chance increasing the amount of days elapsed.
            harmony.PatchAll(typeof(Patches.MeteorPatch));
            // resetting and restricting moon counter
            harmony.PatchAll(typeof(Patches.StartOfRoundPatch));
        }
    }
}
