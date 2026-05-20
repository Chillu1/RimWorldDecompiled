namespace Verse.AI.Group
{
	public class TriggerData_TicksPassedRitual : TriggerData
	{
		public float ticksPassed;

		public override void ExposeData()
		{
			Scribe_Values.Look(ref ticksPassed, "ticksPassed", 0f);
		}
	}
}
