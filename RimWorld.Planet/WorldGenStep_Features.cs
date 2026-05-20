using System;
using System.Linq;
using Verse;

namespace RimWorld.Planet;

public class WorldGenStep_Features : WorldGenStep
{
	public override int SeedPart => 26611132;

	public override void GenerateFresh(string seed, PlanetLayer layer)
	{
		foreach (FeatureDef item in from x in DefDatabase<FeatureDef>.AllDefsListForReading
			orderby x.order, x.index
			select x)
		{
			try
			{
				item.Worker.GenerateWhereAppropriate(layer);
			}
			catch (Exception arg)
			{
				Log.Error($"Could not generate world features of def {item}: {arg}");
			}
		}
	}
}
