using HugsLib.Utils;
using RimWorld;
using UnityEngine;
using Verse;

namespace zzLib
{
    /// <summary>
    /// Auto-filled repository of all external resources referenced in the code
    /// </summary>
    public static class Resources
    {
        [DefOf]
        public static class Job
        {
            public static JobDef zzInstallUpgrade;
        }


        [DefOf]
        public static class Stat
        {
            public static StatDef zzPowerConsumption;
            public static StatDef zzPowerConsumptionRate;
        }

        [DefOf]
        public static class Designation
        {
            public static DesignationDef zzInstallUpgrade;
        }

        [StaticConstructorOnStartup]
        public static class Textures
        {

            public static Texture2D zzUIUpgrade;


            static Textures()
            {

                foreach (var fieldInfo in typeof(Textures).GetFields(HugsLibUtility.AllBindingFlags))
                {
                    if (fieldInfo.IsInitOnly) continue;
                    fieldInfo.SetValue(null, ContentFinder<Texture2D>.Get(fieldInfo.Name));
                }
            }

        }




    }
}
