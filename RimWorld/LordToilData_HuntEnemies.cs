using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToilData_HuntEnemies : LordToilData
	{
		public IntVec3 fallbackLocation = IntVec3.Invalid;

		public override void ExposeData()
		{
			Scribe_Values.Look(ref fallbackLocation, "fallbackLocation", IntVec3.Invalid);
		}
	}
}
