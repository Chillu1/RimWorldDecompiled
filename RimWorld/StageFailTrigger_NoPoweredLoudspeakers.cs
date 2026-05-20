using Verse;

namespace RimWorld
{
	public class StageFailTrigger_NoPoweredLoudspeakers : StageFailTrigger
	{
		public ThingDef onlyIfTargetIsOfDef;

		public const int maxDistance = 12;

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
			foreach (Thing item in thing.Map.listerBuldingOfDefInProximity.GetForCell(thing.Position, 12f, ThingDefOf.Loudspeaker))
			{
				CompPowerTrader compPowerTrader = item.TryGetComp<CompPowerTrader>();
				if (item.GetRoom() == thing.GetRoom() && compPowerTrader.PowerNet != null && compPowerTrader.PowerNet.HasActivePowerSource)
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
		}
	}
}
