using Verse;

namespace RimWorld
{
	public class QuestPart_DelayRandom : QuestPart_Delay
	{
		public IntRange delayTicksRange;

		protected override void Enable(SignalArgs receivedArgs)
		{
			delayTicks = delayTicksRange.RandomInRange;
			base.Enable(receivedArgs);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref delayTicksRange, "delayTicksRange");
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			delayTicksRange = new IntRange(833, 2500);
		}
	}
}
