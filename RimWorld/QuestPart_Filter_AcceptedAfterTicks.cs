using Verse;

namespace RimWorld
{
	public class QuestPart_Filter_AcceptedAfterTicks : QuestPart_Filter
	{
		public int timeTicks;

		protected override bool Pass(SignalArgs args)
		{
			return quest.TicksSinceAccepted > timeTicks;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref timeTicks, "timeTicks", 0);
		}
	}
}
