using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class SitePartWorker_WorkSite_Mining : SitePartWorker_WorkSite
{
	public override IEnumerable<PreceptDef> DisallowedPrecepts => DefDatabase<PreceptDef>.AllDefs.Where((PreceptDef p) => p.disallowMiningCamps);

	public override PawnGroupKindDef WorkerGroupKind => PawnGroupKindDefOf.Miners;

	public override bool CanSpawnOn(PlanetTile tile)
	{
		Hilliness hilliness = Find.WorldGrid[tile].hilliness;
		if ((int)hilliness >= 3)
		{
			return (int)hilliness < 5;
		}
		return false;
	}

	protected override void OnLootChosen(Site site, SitePart sitePart, CampLootThingStruct loot)
	{
		site.customLabel = sitePart.def.label.Formatted(NamedArgumentUtility.Named(loot.thing, "THING"));
	}
}
