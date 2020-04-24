namespace Verse.AI.Group
{
	public class LordToilData_DefendPoint : LordToilData
	{
		public IntVec3 defendPoint = IntVec3.Invalid;

		public float defendRadius = 28f;

		public override void ExposeData()
		{
			Scribe_Values.Look(ref defendPoint, "defendPoint");
			Scribe_Values.Look(ref defendRadius, "defendRadius", 28f);
		}
	}
}
