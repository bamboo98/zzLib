using RimWorld;
using UnityEngine;
using Verse;

namespace zzLib.Comp
{
    class Comp_AutoRepair:ThingComp
    {
        private CompProperties_AutoRepair Prop;

        private CompPowerTrader Power;

        private EnumPowerStrategy PowerStrategy = EnumPowerStrategy.Checking;

        private int MaxHitPoints;

        private int LastRepairTick=-100;

        private float Remainder = 0f;

        private bool TwiceCheck;

        private float IdlePower;
        private float RepairingPower;

        private bool PowerInited = false;

        //存档field
        private bool repairing = false;

        private bool Repairing
        {
            get
            {
                return repairing;
            }
            set
            {
                if (MaxHitPoints == 0) return;
                //打开高耗能
                if (value)
                {
                    Log.Trace("Repair Beginning:"+parent.def.defName);
                    LastRepairTick = GenTicks.TicksGame;
                    Remainder = 0f;
                    if(Power!=null)
                        Power.powerOutputInt = RepairingPower;
                }
                else
                {
                    Log.Trace("Repair Ending:" + parent.def.defName);
                    if (Power != null)
                        Power.powerOutputInt = IdlePower;
                }
                repairing = value;
            }
        }
        
        public bool PowerOn
        {
            get
            {
                return PowerStrategy == EnumPowerStrategy.Offline || Power.PowerOn;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Prop = (CompProperties_AutoRepair)props;
            if (PowerStrategy == EnumPowerStrategy.Checking)
            {
                Power = parent.TryGetComp<CompPowerTrader>();
                PowerStrategy = Power == null ? EnumPowerStrategy.Offline : EnumPowerStrategy.Online;
            }
            if (!parent.def.useHitPoints)
                MaxHitPoints = 0;
            else
                MaxHitPoints = parent.MaxHitPoints;
            IdlePower = -(Prop.BasePower + Prop.RatioOfPowerToHitpoints * MaxHitPoints);
            RepairingPower= IdlePower * Prop.PowerRateOnRapairing;
            Repairing = parent.HitPoints < MaxHitPoints;
            TwiceCheck = parent.def.tickerType == TickerType.Rare;
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);
            if (Repairing||MaxHitPoints == 0) return;
            Repairing = true;
        }

        private void TryRepair()
        {
            try
            {
                if (!PowerInited)
                {
                    PowerInited = true;
                    if (Power != null)
                    {
                        Power.powerOutputInt = Repairing ? RepairingPower : IdlePower;
                    }
                }
                if (!Repairing || !PowerOn) return;
                int cur = parent.HitPoints;
                if (MaxHitPoints <= cur)
                {
                    Repairing = false;
                    return;
                }
                float heal = MaxHitPoints * Prop.RepairRatePerSec;
                int gt = GenTicks.TicksGame;
                heal *= (gt - LastRepairTick).TicksToSeconds();
                heal += Remainder;
                Remainder = heal % 1;
                if (heal >= 1)
                {
                    parent.HitPoints = Mathf.Min(MaxHitPoints, cur + Mathf.FloorToInt(heal));
                }
                LastRepairTick = gt;
                if(TwiceCheck|| MaxHitPoints <= parent.HitPoints)
                {
                    Repairing = false;
                }
            }
            catch (System.Exception e)
            {
                Log.Warning(e.Message);
                throw;
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            TryRepair();
        }
        public override void CompTickRare()
        {
            base.CompTickRare();
            TryRepair();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref LastRepairTick, "LastRepairTick"); 
            Scribe_Values.Look(ref Remainder, "Remainder");
        }

    }

    public enum EnumPowerStrategy
    {
        Online,
        Offline,
        Checking
    }
}
