﻿using HarmonyLib;
using RimWorld;
using Verse;


namespace zzLib.Patch
{
    /// <summary>
    /// Allows types extending CompPowerTrader to be recognized as power grid connectables
    /// </summary>
    [HarmonyPatch(typeof(ThingDef))]
    [HarmonyPatch("ConnectToPower", MethodType.Getter)]
    internal class ThingDef_ConnectToPower_Patch
    {
        [HarmonyPostfix]
        public static void AllowPolymorphicComps(ThingDef __instance, ref bool __result)
        {
            //Log.Message("patched!");
            if (!__instance.EverTransmitsPower)
            {
                for (var i = 0; i < __instance.comps.Count; i++)
                {
                    if (typeof(CompPowerTrader).IsAssignableFrom(__instance.comps[i]?.compClass))
                    {
                        __result = true;
                        return;
                    }
                }
            }
        }
    }

}
