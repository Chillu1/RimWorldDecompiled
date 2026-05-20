using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Alert_MechThreat : Alert_Scenario
	{
		public int raidTick;

		private bool Red => Find.TickManager.TicksGame > raidTick - 60000;

		private bool Critical => Find.TickManager.TicksGame > raidTick;

		protected override Color BGColor
		{
			get
			{
				if (!Red)
				{
					return Color.clear;
				}
				return Alert_Critical.BgColor();
			}
		}

		public override AlertReport GetReport()
		{
			return AlertReport.Active;
		}

		public override string GetLabel()
		{
			if (Critical)
			{
				return "AlertMechanoidThreatCritical".Translate();
			}
			return "AlertMechanoidThreat".Translate() + ": " + (raidTick - Find.TickManager.TicksGame).ToStringTicksToPeriod(allowSeconds: false, shortForm: false, canUseDecimals: false);
		}

		public override TaggedString GetExplanation()
		{
			if (Critical)
			{
				return "AlertMechanoidThreatCriticalDesc".Translate();
			}
			return "AlertMechanoidThreatDesc".Translate();
		}
	}
}
