using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToilData_SanguophageMeeting : LordToilData
	{
		public int ticksInMeeting;

		public override void ExposeData()
		{
			Scribe_Values.Look(ref ticksInMeeting, "ticksInMeeting", 0);
		}
	}
}
