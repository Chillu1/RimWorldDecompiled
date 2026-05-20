using RimWorld;

namespace Verse;

public class PollutionInfo : MapComponent
{
	private float pollutionAtGen;

	public PollutionInfo(Map map)
		: base(map)
	{
	}

	public override void MapGenerated()
	{
		if (ModsConfig.BiotechActive)
		{
			pollutionAtGen = map.pollutionGrid.TotalPollution;
		}
	}

	public override void MapRemoved()
	{
		if (ModsConfig.BiotechActive)
		{
			float num = (float)map.pollutionGrid.TotalPollution - pollutionAtGen;
			CompProperties_DissolutionEffectPollution compProperties = ThingDefOf.Wastepack.GetCompProperties<CompProperties_DissolutionEffectPollution>();
			int amount = (int)(num / (float)compProperties.cellsToPollutePerDissolution);
			if (num > 0f)
			{
				CompDissolutionEffect_Goodwill.AddWorldDissolutionEvent(amount, map.Tile);
			}
		}
	}

	public override void ExposeData()
	{
		if (ModsConfig.BiotechActive)
		{
			Scribe_Values.Look(ref pollutionAtGen, "pollutionAtGen", 0f);
		}
	}
}
