using Verse;

namespace RimWorld
{
	public class StageFailTrigger_TargetThingMissing : StageFailTrigger
	{
		public ThingDef onlyIfTargetIsOfDef;

		public override bool Failed(LordJob_Ritual ritual, TargetInfo spot, TargetInfo focus)
		{
			if (onlyIfTargetIsOfDef != null && ritual.selectedTarget.Thing?.def != onlyIfTargetIsOfDef)
			{
				return false;
			}
			if (ritual.selectedTarget.Thing != null)
			{
				return ritual.selectedTarget.ThingDestroyed;
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
