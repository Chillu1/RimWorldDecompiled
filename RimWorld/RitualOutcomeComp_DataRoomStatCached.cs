using Verse;

namespace RimWorld
{
	public class RitualOutcomeComp_DataRoomStatCached : RitualOutcomeComp_Data
	{
		public float? startingVal;

		public override void Reset()
		{
			base.Reset();
			startingVal = null;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref startingVal, "startingVal");
		}
	}
}
