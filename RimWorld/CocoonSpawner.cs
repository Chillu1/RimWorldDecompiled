using Verse;

namespace RimWorld
{
	public class CocoonSpawner : TunnelHiveSpawner
	{
		public ThingDef cocoon;

		public int groupID = -1;

		protected override void Spawn(Map map, IntVec3 loc)
		{
			Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(cocoon), loc, map);
			thing.SetFaction(Faction.OfInsects);
			thing.questTags = questTags;
			if (groupID >= 0)
			{
				CompWakeUpDormant compWakeUpDormant = thing.TryGetComp<CompWakeUpDormant>();
				if (compWakeUpDormant != null)
				{
					compWakeUpDormant.groupID = groupID;
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref cocoon, "cocoon");
			Scribe_Values.Look(ref groupID, "groupID", -1);
		}
	}
}
