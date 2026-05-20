using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class AbandonedArchotechStructures : WorldObject
	{
		public WorldObjectDef worldObjectDef;

		public List<Building> archotechStructures = new List<Building>();

		public Settlement GenerateSettlementAndDestroy()
		{
			Destroy();
			ArchotechSettlement archotechSettlement;
			if (worldObjectDef != null)
			{
				archotechSettlement = (ArchotechSettlement)WorldObjectMaker.MakeWorldObject(worldObjectDef);
			}
			else
			{
				archotechSettlement = (ArchotechSettlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement_SecondArchonexusCycle);
				Log.Warning("No archotech settlement configured for abandoned archotech structures, using default settlement def.");
			}
			archotechSettlement.existingBuildings.AddRange(archotechStructures);
			return archotechSettlement;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref worldObjectDef, "worldObjectDef");
			Scribe_Collections.Look(ref archotechStructures, "archotechStructures", LookMode.Deep);
		}
	}
}
