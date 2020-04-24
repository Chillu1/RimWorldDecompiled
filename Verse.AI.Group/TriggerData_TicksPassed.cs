namespace Verse.AI.Group
{
	public class TriggerData_TicksPassed : TriggerData
	{
		public int ticksPassed;

		public override void ExposeData()
		{
			Scribe_Values.Look(ref ticksPassed, "ticksPassed", 0);
		}
	}
}
