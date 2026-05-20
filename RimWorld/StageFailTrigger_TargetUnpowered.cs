using Verse;

namespace RimWorld
{
	public class StageFailTrigger_TargetUnpowered : StageFailTrigger
	{
		public ThingDef onlyIfTargetIsOfDef;

		public override bool Failed(LordJob_Ritual ritual, TargetInfo spot, TargetInfo focus)
		{
			if (onlyIfTargetIsOfDef != null && ritual.selectedTarget.Thing?.def != onlyIfTargetIsOfDef)
			{
				return false;
			}
			CompPowerTrader compPowerTrader = ritual.selectedTarget.Thing?.TryGetComp<CompPowerTrader>();
			if (compPowerTrader != null)
			{
				return !compPowerTrader.PowerOn;
			}
			return true;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref onlyIfTargetIsOfDef, "onlyIfTargetIsOfDef");
		}
	}
}
