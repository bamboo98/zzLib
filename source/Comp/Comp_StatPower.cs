using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace zzLib.Comp
{
    /// <summary>
    /// Enables power consumption to be pulled from a stat value, which allows it to be affected by upgrade comps
    /// </summary>
    public class CompStatPower : CompPowerTrader
    {
        private const int UpdateEveryTicks = 30;

        protected virtual float PowerConsumption
        {
            get { return parent.GetStatValue(Resources.Stat.zzPowerConsumption) * parent.GetStatValue(Resources.Stat.zzPowerConsumptionRate); }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            SetUpPowerVars();
        }

        public override void ReceiveCompSignal(string signal)
        {
            //Log.Message("power接收到信号" + signal);
            base.ReceiveCompSignal(signal);
            if (signal == CompUpgrade.UpgradeCompleteSignal) SetUpPowerVars();
        }

        public override void SetUpPowerVars()
        {
            // allows the comp to switch from consumer to producer
            var prevDefValue = Props.basePowerConsumption;
            Props.basePowerConsumption = PowerConsumption;
            //Log.Message("power设置:" + PowerConsumption);
            base.SetUpPowerVars();
            Props.basePowerConsumption = prevDefValue;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (Find.TickManager.TicksGame % UpdateEveryTicks == 0)
            {
                SetUpPowerVars();
            }
        }
    }
}
