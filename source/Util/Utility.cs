using System;
using System.Collections.Generic;
using System.Linq;
using HugsLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace zzLib.Util
{
    /// <summary>
    /// For comps that want to be notified about their parent doing something that draws power.
    /// </summary>
    public interface IPowerUseNotified
    {
        void ReportPowerUse(float duration = 1f);
    }
    /// <summary>
    /// A place for common functions and utilities used by the mod.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class RemoteTechUtility
    {
        public const int DefaultChannel = 1;

        public static Comp.CompUpgrade FirstUpgradeableComp(this Thing t)
        {
            if (t is ThingWithComps comps)
            {
                for (var i = 0; i < comps.AllComps.Count; i++)
                {
                    if (comps.AllComps[i] is Comp.CompUpgrade comp && comp.WantsWork)
                    {
                        return comp;
                    }
                }
            }
            return null;
        }

        public static Comp.CompUpgrade TryGetUpgrade(this Thing t, string upgradeReferenceId)
        {
            if (t is ThingWithComps comps)
            {
                for (var i = 0; i < comps.AllComps.Count; i++)
                {
                    if (comps.AllComps[i] is Comp.CompUpgrade comp && comp.Props.referenceId == upgradeReferenceId)
                    {
                        return comp;
                    }
                }
            }
            return null;
        }

        public static bool IsUpgradeCompleted(this Thing t, string upgradeReferenceId)
        {
            var upgrade = t.TryGetUpgrade(upgradeReferenceId);
            return upgrade != null && upgrade.Complete;
        }


        public static void ReportPowerUse(ThingWithComps thing, float duration = 1f)
        {
            for (var i = 0; i < thing.AllComps.Count; i++)
            {
                if (thing.AllComps[i] is IPowerUseNotified comp) comp.ReportPowerUse(duration);
            }
        }

        public static T RequireComp<T>(this ThingWithComps thing) where T : ThingComp
        {
            var c = thing.GetComp<T>();
            if (c == null) Log.Error($"{thing.GetType().Name} requires ThingComp of type {nameof(T)} in def {thing.def.defName}");
            return c;
        }

        public static T RequireComponent<T>(this ThingWithComps thing, T component)
        {
            if (component == null) Log.Error($"{thing.GetType().Name} requires {nameof(T)} in def {thing.def.defName}");
            return component;
        }

        public static T RequireComponent<T>(this ThingComp comp, T component)
        {
            if (component == null) Log.Error($"{comp.GetType().Name} requires {nameof(T)} in def {comp.parent.def.defName}");
            return component;
        }

        public static void RequireTicker(this ThingComp comp, TickerType type)
        {
            if (comp.parent.def.tickerType != type) Log.Error($"{comp.GetType().Name} requires tickerType:{type} in def {comp.parent.def.defName}");
        }

        public static CachedValue<float> GetCachedStat(this Thing thing, StatDef stat, int recacheInterval = GenTicks.TicksPerRealSecond)
        {
            return new CachedValue<float>(() => thing.GetStatValue(stat), recacheInterval);
        }

        public static bool ApproximatelyEquals(this float value1, float value2, float tolerance = float.Epsilon)
        {
            return Math.Abs(value1 - value2) < tolerance;
        }
    }
}
