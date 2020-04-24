using RimWorld;

namespace Verse.AI.Group
{
	public class Trigger_ChanceOnPlayerHarmNPCBuilding : Trigger
	{
		private float chance = 1f;

		public Trigger_ChanceOnPlayerHarmNPCBuilding(float chance)
		{
			this.chance = chance;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.BuildingDamaged && signal.dinfo.Def.ExternalViolenceFor(signal.thing) && signal.thing.def.category == ThingCategory.Building && signal.dinfo.Instigator != null && signal.dinfo.Instigator.Faction == Faction.OfPlayer && signal.thing.Faction != Faction.OfPlayer && Rand.Value < chance)
			{
				return true;
			}
			return false;
		}
	}
}
