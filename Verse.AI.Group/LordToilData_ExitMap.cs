namespace Verse.AI.Group
{
	public class LordToilData_ExitMap : LordToilData
	{
		public LocomotionUrgency locomotion;

		public bool canDig;

		public bool interruptCurrentJob;

		public override void ExposeData()
		{
			Scribe_Values.Look(ref locomotion, "locomotion", LocomotionUrgency.None);
			Scribe_Values.Look(ref canDig, "canDig", defaultValue: false);
			Scribe_Values.Look(ref interruptCurrentJob, "interruptCurrentJob", defaultValue: false);
		}
	}
}
