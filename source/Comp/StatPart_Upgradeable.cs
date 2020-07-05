using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace zzLib.Comp
{
    /// <summary>
    /// Required for CompUpgrade to modify arbitrary stats.
    /// This is automatically added to StatDefs that have been found to be used in a CompProperties_Upgrade.
    /// </summary>
    /// <see cref="RemoteTechController.InjectUpgradeableStatParts"/>
    public class StatPart_Upgradeable : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is ThingWithComps tcomps)
            {
                for (var i = 0; i < tcomps.AllComps.Count; i++)
                {
                    if (tcomps.AllComps[i] is CompUpgrade upgrade)
                    {
                        if (upgrade.anyComplete)
                        {
                            var mod = upgrade.TryGetStatModifier(parentStat);
                            if (mod != null)
                            {
                                val *= Mathf.Pow(mod.value, upgrade.UpgradeCount);
                            }
                        }
                    }
                }
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            StringBuilder builder = null;
            if (req.Thing is ThingWithComps tcomps)
            {
                for (var i = 0; i < tcomps.AllComps.Count; i++)
                {
                    if (tcomps.AllComps[i] is CompUpgrade upgrade && upgrade.anyComplete)
                    {
                        var mod = upgrade.TryGetStatModifier(parentStat);
                        if (mod != null)
                        {
                            if (builder == null)
                            {
                                builder = new StringBuilder("Upgrade_statModifierCategory".Translate());
                                builder.AppendLine();
                            }
                            builder.Append("    ");
                            builder.Append(upgrade.Props.label.CapitalizeFirst());
                            if (upgrade.UpgradeCount > 1)
                                builder.Append("(" + upgrade.UpgradeCount + "): ");
                            else
                                builder.Append(": ");
                            if (upgrade.anyComplete)
                                builder.Append(mod.stat.Worker.ValueToString(Mathf.Pow(mod.value, upgrade.UpgradeCount), false, ToStringNumberSense.Factor));
                            else
                                //;
                                builder.Append(mod.ToStringAsFactor);
                        }
                    }
                }
            }
            return builder?.ToString();
        }
    }
    public class StatPart_Upgradeable2 : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is ThingWithComps tcomps)
            {
                for (var i = 0; i < tcomps.AllComps.Count; i++)
                {
                    if (tcomps.AllComps[i] is CompUpgrade upgrade)
                    {
                        if (upgrade.anyComplete)
                        {
                            var mod = upgrade.TryGetStatModifierOffset(parentStat);
                            if (mod != null)
                            {
                                val += mod.value * (float)upgrade.UpgradeCount;
                            }
                        }
                    }
                }
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            StringBuilder builder = null;
            if (req.Thing is ThingWithComps tcomps)
            {
                for (var i = 0; i < tcomps.AllComps.Count; i++)
                {
                    if (tcomps.AllComps[i] is CompUpgrade upgrade && upgrade.anyComplete)
                    {
                        var mod = upgrade.TryGetStatModifierOffset(parentStat);
                        if (mod != null)
                        {
                            if (builder == null)
                            {
                                builder = new StringBuilder("Upgrade_statModifierCategory".Translate());
                                builder.AppendLine();
                            }
                            builder.Append("    ");
                            builder.Append(upgrade.Props.label.CapitalizeFirst());
                            if (upgrade.UpgradeCount > 1)
                                builder.Append("(" + upgrade.UpgradeCount + "): ");
                            else
                                builder.Append(": ");
                            if (upgrade.anyComplete)
                                builder.Append(mod.stat.Worker.ValueToString(mod.value * (float)upgrade.UpgradeCount, false, ToStringNumberSense.Offset));
                            else
                                //;
                                builder.Append(mod.ValueToStringAsOffset);
                        }
                    }
                }
            }
            return builder?.ToString();
        }
    }

    public class StatModifierOffset : StatModifier
    {
    }
}
