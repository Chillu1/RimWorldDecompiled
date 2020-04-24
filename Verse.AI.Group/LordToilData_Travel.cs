namespace Verse.AI.Group
{
	public class LordToilData_Travel : LordToilData
	{
		public IntVec3 dest;

		public override void ExposeData()
		{
			Scribe_Values.Look(ref dest, "dest");
		}
	}
}
