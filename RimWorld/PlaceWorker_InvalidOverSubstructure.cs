using System.Linq;
using Verse;

namespace RimWorld;

public class PlaceWorker_InvalidOverSubstructure : PlaceWorker
{
	public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return AcceptanceReport.WasAccepted;
		}
		foreach (IntVec3 item in GenAdj.OccupiedRect(loc, rot, checkingDef.Size))
		{
			if (!item.InBounds(map))
			{
				continue;
			}
			TerrainDef terrainDef = map.terrainGrid.FoundationAt(item);
			if (terrainDef != null && terrainDef.IsSubstructure)
			{
				return "MessageCannotPlaceOverSubstructure".Translate();
			}
			foreach (Blueprint item2 in map.thingGrid.ThingsListAt(item).OfType<Blueprint>())
			{
				if (item2.def.entityDefToBuild is TerrainDef { IsSubstructure: not false })
				{
					return "MessageCannotPlaceOverSubstructure".Translate();
				}
			}
		}
		return AcceptanceReport.WasAccepted;
	}
}
