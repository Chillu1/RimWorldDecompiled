using System;
using System.Linq;
using Verse;

namespace RimWorld.Planet
{
	public class WorldGenStep_Features : WorldGenStep
	{
		public override int SeedPart => 711240483;

		public override void GenerateFresh(string seed)
		{
			Find.World.features = new WorldFeatures();
			foreach (FeatureDef item in from x in DefDatabase<FeatureDef>.AllDefsListForReading
				orderby x.order, x.index
				select x)
			{
				try
				{
					item.Worker.GenerateWhereAppropriate();
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat("Could not generate world features of def ", item, ": ", ex));
				}
			}
		}
	}
}
