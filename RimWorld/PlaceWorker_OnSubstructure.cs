using System;
using Verse;

namespace RimWorld;

[Obsolete("Use Substructure affordance instead.")]
public class PlaceWorker_OnSubstructure : PlaceWorker
{
	public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
	{
		foreach (IntVec3 item in GenAdj.OccupiedRect(loc, rot, checkingDef.Size))
		{
			if (item.InBounds(map))
			{
				TerrainDef terrainDef = map.terrainGrid.FoundationAt(item);
				if (terrainDef == null || !terrainDef.IsSubstructure)
				{
					return "MessageMustBePlacedOnSubstructure".Translate();
				}
			}
		}
		return AcceptanceReport.WasAccepted;
	}
}
