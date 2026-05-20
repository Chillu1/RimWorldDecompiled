using Verse;

namespace RimWorld
{
	public class QuestPart_Filter_AllRequiredThingsLoaded : QuestPart_Filter
	{
		public Thing shuttle;

		protected override bool Pass(SignalArgs args)
		{
			if (shuttle == null)
			{
				return false;
			}
			return shuttle.TryGetComp<CompShuttle>()?.AllRequiredThingsLoaded ?? false;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref shuttle, "shuttle");
		}
	}
}
