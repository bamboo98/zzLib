using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using RimWorld;
using HugsLib;

namespace zzLib
{
    public class Manager : Mod
    {
        public ModContentPack Pack { get; }

        private static Setting setting = new Setting();

        public Manager(ModContentPack content) : base(content)
        {
            Pack = content;
            setting = GetSettings<Setting>();
        }
        public override string SettingsCategory() => Pack.Name;
        public override void DoSettingsWindowContents(Rect inRect) => setting.DoWindowContents(inRect);

    }

    public static class Log
    {
        public static void Trace(string msg, bool force = false)
        {
            if (!Setting.DEBUG_MODE) return;
            Verse.Log.Message("[zzLib Trace] " + msg, force);
        }
        public static void Info(string msg, bool force = false)
        {
            if (!Setting.DEBUG_MODE) return;
            Verse.Log.Message("[zzLib Info] " + msg, force);
        }
        public static void Message(string msg, bool force = false)
        {
            Verse.Log.Message("[zzLib Message] " + msg, force);
        }
        public static void Warning(string msg, bool force = false)
        {
            Verse.Log.Warning("[zzLib Warn] " + msg, force);
        }
        public static void Error(string msg, bool force = false)
        {
            Verse.Log.Error("[zzLib Error] " + msg, force);
        }
    }


    public class Setting : ModSettings
    {
        //存档field
        public static bool DEBUG_MODE=false;

        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref DEBUG_MODE, "DEBUG_MODE", false, true);

        }


        public void DoWindowContents(Rect inRect)
        {
            var list = new Listing_Standard()
            {
                ColumnWidth = inRect.width
            };
            list.Begin(inRect);

            list.CheckboxLabeled("DEBUG_MODE".Translate(), ref DEBUG_MODE, "DEBUG_MODE_TOOLTIP".Translate());


            list.End();
        }



    }


    class ManagerBase : ModBase
    {
        public static ManagerBase Instance { get; private set; }
        public override string ModIdentifier
        {
            get { return "zzLib"; }
        }
        public ManagerBase()
        {
            Instance = this;
        }


        public override void DefsLoaded()
        {
            InjectUpgradeableStatParts();
        }

        /// <summary>
        /// Add StatPart_Upgradeable to all stats that are used in any CompProperties_Upgrade
        /// </summary>
        private void InjectUpgradeableStatParts()
        {
            try
            {
                var relevantStats = new HashSet<StatDef>();
                var relevantStats2 = new HashSet<StatDef>();
                var allThings = DefDatabase<ThingDef>.AllDefs.ToArray();
                for (var i = 0; i < allThings.Length; i++)
                {
                    var def = allThings[i];
                    if (def.comps.Count > 0)
                    {
                        for (int j = 0; j < def.comps.Count; j++)
                        {
                            var comp = def.comps[j];
                            if (comp is Comp.CompProperties_Upgrade upgradeProps)
                            {
                                foreach (var upgradeProp in upgradeProps.statModifiers)
                                {
                                    relevantStats.Add(upgradeProp.stat);
                                }
                                foreach (var upgradeProp in upgradeProps.statModifiersOffset)
                                {
                                    relevantStats2.Add(upgradeProp.stat);
                                }
                            }
                        }
                    }
                }
                foreach (var stat in relevantStats2)
                {
                    var parts = stat.parts ?? (stat.parts = new List<StatPart>());
                    parts.Add(new Comp.StatPart_Upgradeable2 { parentStat = stat });
                }
                foreach (var stat in relevantStats)
                {
                    var parts = stat.parts ?? (stat.parts = new List<StatPart>());
                    parts.Add(new Comp.StatPart_Upgradeable { parentStat = stat });
                }
            }
            catch (Exception e)
            {
                Logger.ReportException(e);
            }
        }
    }


}
