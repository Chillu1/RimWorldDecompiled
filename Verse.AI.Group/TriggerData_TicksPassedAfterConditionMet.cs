namespace Verse.AI.Group
{
	public class TriggerData_TicksPassedAfterConditionMet : TriggerData_TicksPassed
	{
		public bool conditionMet;

		public override void ExposeData()
		{
			Scribe_Values.Look(ref conditionMet, "conditionMet", defaultValue: false);
		}
	}
}
