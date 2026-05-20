using Verse;

namespace RimWorld
{
	public class StageFailTrigger_NoThingPresent : StageFailTrigger
	{
		public ThingDef onlyIfTargetIsOfDef;

		public int maxDistance = 12;

		public ThingDef thingDef;

		public override bool Failed(LordJob_Ritual ritual, TargetInfo spot, TargetInfo focus)
		{
			if (onlyIfTargetIsOfDef != null && ritual.selectedTarget.Thing?.def != onlyIfTargetIsOfDef)
			{
				return false;
			}
			Thing thing = ritual.selectedTarget.Thing;
			if (thing == null || thing.Destroyed)
			{
				return true;
			}
			foreach (Thing item in thing.Map.listerBuldingOfDefInProximity.GetForCell(thing.Position, maxDistance, thingDef))
			{
				if (item.GetRoom() == thing.GetRoom())
				{
					return false;
				}
			}
			return true;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref onlyIfTargetIsOfDef, "onlyIfTargetIsOfDef");
			Scribe_Defs.Look(ref thingDef, "thingDef");
			Scribe_Values.Look(ref maxDistance, "maxDistance", 0);
		}
	}
}
