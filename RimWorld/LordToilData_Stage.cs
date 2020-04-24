using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToilData_Stage : LordToilData
	{
		public IntVec3 stagingPoint;

		public override void ExposeData()
		{
			Scribe_Values.Look(ref stagingPoint, "stagingPoint");
		}
	}
}
