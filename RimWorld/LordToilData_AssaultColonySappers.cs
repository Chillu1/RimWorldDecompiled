using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToilData_AssaultColonySappers : LordToilData
	{
		public IntVec3 sapperDest = IntVec3.Invalid;

		public override void ExposeData()
		{
			Scribe_Values.Look(ref sapperDest, "sapperDest");
		}
	}
}
