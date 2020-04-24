namespace Verse
{
	public struct CoverInfo
	{
		private Thing thingInt;

		private float blockChanceInt;

		public Thing Thing => thingInt;

		public float BlockChance => blockChanceInt;

		public static CoverInfo Invalid => new CoverInfo(null, -999f);

		public CoverInfo(Thing thing, float blockChance)
		{
			thingInt = thing;
			blockChanceInt = blockChance;
		}
	}
}
