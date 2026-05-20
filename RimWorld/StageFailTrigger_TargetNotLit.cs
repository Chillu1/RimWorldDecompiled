using Verse;

namespace RimWorld
{
	public class StageFailTrigger_TargetNotLit : StageFailTrigger
	{
		public ThingDef onlyIfTargetIsOfDef;

		public override bool Failed(LordJob_Ritual ritual, TargetInfo spot, TargetInfo focus)
		{
			if (onlyIfTargetIsOfDef != null && ritual.selectedTarget.Thing?.def != onlyIfTargetIsOfDef)
			{
				return false;
			}
			CompRefuelable compRefuelable = ritual.selectedTarget.Thing?.TryGetComp<CompRefuelable>();
			if (compRefuelable != null)
			{
				return !compRefuelable.HasFuel;
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
