using Verse;

namespace RimWorld
{
	public class DrugPolicyEntry : IExposable
	{
		public ThingDef drug;

		public bool allowedForAddiction;

		public bool allowedForJoy;

		public bool allowScheduled;

		public float daysFrequency = 1f;

		public float onlyIfMoodBelow = 1f;

		public float onlyIfJoyBelow = 1f;

		public int takeToInventory;

		public string takeToInventoryTempBuffer;

		public void ExposeData()
		{
			Scribe_Defs.Look(ref drug, "drug");
			Scribe_Values.Look(ref allowedForAddiction, "allowedForAddiction", defaultValue: false);
			Scribe_Values.Look(ref allowedForJoy, "allowedForJoy", defaultValue: false);
			Scribe_Values.Look(ref allowScheduled, "allowScheduled", defaultValue: false);
			Scribe_Values.Look(ref daysFrequency, "daysFrequency", 1f);
			Scribe_Values.Look(ref onlyIfMoodBelow, "onlyIfMoodBelow", 1f);
			Scribe_Values.Look(ref onlyIfJoyBelow, "onlyIfJoyBelow", 1f);
			Scribe_Values.Look(ref takeToInventory, "takeToInventory", 0);
		}
	}
}
