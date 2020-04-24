namespace Verse
{
	public abstract class WorldGenStep
	{
		public WorldGenStepDef def;

		public abstract int SeedPart
		{
			get;
		}

		public abstract void GenerateFresh(string seed);

		public virtual void GenerateWithoutWorldData(string seed)
		{
			GenerateFresh(seed);
		}

		public virtual void GenerateFromScribe(string seed)
		{
		}
	}
}
