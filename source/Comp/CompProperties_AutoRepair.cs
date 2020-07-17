using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace zzLib.Comp
{
    public class CompProperties_AutoRepair :CompProperties
    {
        //每点墙壁血量提高多少待机耗能
        public float RatioOfPowerToHitpoints = 0.01f;
        //待机基础耗能
        public float BasePower = 20;
        //修复速度,百分比/秒
        public float RepairRatePerSec = 0.03f;
        //修复时的耗能倍数
        public float PowerRateOnRapairing = 30f;


        public CompProperties_AutoRepair()
        {
            compClass = typeof(Comp_AutoRepair);
        }
        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            if (parentDef.tickerType != TickerType.Normal && parentDef.tickerType!= TickerType.Rare) yield return "AutoRepairNeedTickTypeNormal".Translate(parentDef.defName);
            
        }


    }
}
