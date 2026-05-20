using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class SitePartWorker_WorkSite_Logging : SitePartWorker_WorkSite
{
	public override IEnumerable<PreceptDef> DisallowedPrecepts => DefDatabase<PreceptDef>.AllDefs.Where((PreceptDef p) => p.disallowLoggingCamps);

	public override PawnGroupKindDef WorkerGroupKind => PawnGroupKindDefOf.Loggers;

	public override bool CanSpawnOn(PlanetTile tile)
	{
		return Find.WorldGrid[tile].PrimaryBiome.TreeDensity >= BiomeDefOf.Tundra.TreeDensity;
	}
}
