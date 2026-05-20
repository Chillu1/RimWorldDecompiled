using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class StockGenerator_Tomes : StockGenerator_SingleDef
{
	public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
	{
		if (!ModsConfig.AnomalyActive || !Find.Storyteller.difficulty.AnomalyPlaystyleDef.enableAnomalyContent)
		{
			yield break;
		}
		foreach (Thing item in StockGeneratorUtility.TryMakeForStock(ThingDefOf.Tome, RandomCountOf(ThingDefOf.Tome), faction))
		{
			yield return item;
		}
	}

	public override bool HandlesThingDef(ThingDef thingDef)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (!Find.Storyteller.difficulty.AnomalyPlaystyleDef.enableAnomalyContent)
		{
			return false;
		}
		return thingDef == ThingDefOf.Tome;
	}

	public override IEnumerable<string> ConfigErrors(TraderKindDef parentDef)
	{
		yield break;
	}
}
