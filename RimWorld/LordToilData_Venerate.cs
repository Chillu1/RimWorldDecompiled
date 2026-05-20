using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToilData_Venerate : LordToilData
	{
		public int currentNearVeneratorTicks = -1;

		public int lastNearVeneratorIndex = -1;

		public override void ExposeData()
		{
			Scribe_Values.Look(ref currentNearVeneratorTicks, "currentNearVeneratorTicks", 0);
			Scribe_Values.Look(ref lastNearVeneratorIndex, "lastNearVeneratorIndex", 0);
		}
	}
}
