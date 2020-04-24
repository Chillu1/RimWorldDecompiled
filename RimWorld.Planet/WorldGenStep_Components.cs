using Verse;

namespace RimWorld.Planet
{
	public class WorldGenStep_Components : WorldGenStep
	{
		public override int SeedPart => 508565678;

		public override void GenerateFresh(string seed)
		{
			Find.World.ConstructComponents();
		}

		public override void GenerateWithoutWorldData(string seed)
		{
			GenerateFromScribe(seed);
		}

		public override void GenerateFromScribe(string seed)
		{
			Find.World.ConstructComponents();
			Find.World.ExposeComponents();
		}
	}
}
