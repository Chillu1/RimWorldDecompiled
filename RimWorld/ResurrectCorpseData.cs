using Verse;

namespace RimWorld
{
	public class ResurrectCorpseData : IExposable
	{
		public Corpse corpse;

		public IntVec3 castPosition;

		public ResurrectCorpseData()
		{
		}

		public ResurrectCorpseData(Corpse corpse, IntVec3 castPosition)
		{
			this.corpse = corpse;
			this.castPosition = castPosition;
		}

		public void ExposeData()
		{
			Scribe_References.Look(ref corpse, "corpse");
			Scribe_Values.Look(ref castPosition, "castPosition", IntVec3.Invalid);
		}
	}
}
