namespace Verse.AI.Group
{
	public class TriggerData_Reason : TriggerData
	{
		public string reason;

		public override void ExposeData()
		{
			Scribe_Values.Look(ref reason, "reason");
		}
	}
}
