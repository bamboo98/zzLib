using UnityEngine;
using Verse;
using zzLib.Util;

namespace zzLib.Comp
{
    /// <summary>
    /// Stat-based power consumption that will switch to Idle mode when the parent device is not in use.
    /// By default looks for pawns on the interaction cells, but can be called directly to report in-use status.
    /// Requires Normal ticks.
    /// </summary>
    public class CompStatPowerIdle : CompStatPower, IPowerUseNotified
    {
        private const string IdlePowerUpgradeReferenceId = "zzIdlePower";
        //待机状态耗电量为10%
        private const float IdlePowerConsumptionRate = 0.1f;
        private const int InteractionCellPollIntervalTicks = 20;

        private bool hasUpgrade;

        //不使用自动电源管理
        public bool _disableAutoCheck = false;
        private float _PowerConsumptionRate = 1;
        public float _powerConsumptionRate
        {
            get { return _PowerConsumptionRate; }
            set
            {
                _PowerConsumptionRate = value;
                if (hasUpgrade)
                    SetUpPowerVars();
            }
        }

        // saved
        private int _highPowerTicks;

        // we can't override PowerOutput in ComPowerTrader, so we go the sneaky way and use SetUpPowerVars
        private int HighPowerTicksLeft
        {
            get { return _highPowerTicks; }
            set
            {
                var wasIdle = _highPowerTicks > 0;
                var isIdle = value > 0;
                _highPowerTicks = Mathf.Max(0, value);
                if (wasIdle != isIdle)
                {
                    SetUpPowerVars();
                }
            }
        }

        protected override float PowerConsumption
        {
            get
            {
                if (_disableAutoCheck && hasUpgrade)
                {
                    return _powerConsumptionRate * base.PowerConsumption;
                }
                else
                    return IdlePowerMode ? (base.PowerConsumption * IdlePowerConsumptionRate) : base.PowerConsumption;
            }
        }

        private bool IdlePowerMode
        {
            get { return hasUpgrade && HighPowerTicksLeft == 0; }
        }

        private bool HasIdlePowerUpgrade
        {
            get { return parent.IsUpgradeCompleted(IdlePowerUpgradeReferenceId); }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            //Log.Message("触发插件安装");
            base.PostSpawnSetup(respawningAfterLoad);
            hasUpgrade = HasIdlePowerUpgrade;
            this.RequireTicker(TickerType.Normal);
        }

        public override void CompTick()
        {
            if (_disableAutoCheck) return;

            if (HighPowerTicksLeft > 0) HighPowerTicksLeft--;
            if (parent.def.hasInteractionCell && GenTicks.TicksGame % InteractionCellPollIntervalTicks == 0)
            {
                var pawnInCell = parent.InteractionCell.GetFirstPawn(parent.Map);
                if (pawnInCell != null && pawnInCell.IsColonist)
                {
                    ReportPowerUse(3f);
                }
            }
        }

        public override void PostExposeData()
        {
            //Log.Message("数据通信");
            base.PostExposeData();
            Scribe_Values.Look(ref _highPowerTicks, "highPowerTicks");
        }

        public override void ReceiveCompSignal(string signal)
        {
            //Log.Message("poweridie接收到信号" + signal);
            hasUpgrade = HasIdlePowerUpgrade;
            base.ReceiveCompSignal(signal);
        }

        /// <summary>
        /// 设置开始使用电力(结束待机)
        /// </summary>
        /// <param name="duration">秒数</param>
        public void ReportPowerUse(float duration = 1f)
        {
            //Log.Message("设置高耗能秒数" + duration);
            HighPowerTicksLeft = Mathf.Max(HighPowerTicksLeft, duration.SecondsToTicks());
        }
    }
}
