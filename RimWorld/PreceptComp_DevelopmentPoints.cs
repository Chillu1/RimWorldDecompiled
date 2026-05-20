using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PreceptComp_DevelopmentPoints : PreceptComp
	{
		public HistoryEventDef eventDef;

		public int points;

		[MustTranslate]
		public string eventLabel;

		private string Label
		{
			get
			{
				if (eventLabel.NullOrEmpty())
				{
					return eventDef.label;
				}
				return eventLabel.Formatted().Resolve();
			}
		}

		private string LabelCap => (!eventLabel.NullOrEmpty()) ? eventLabel.Formatted().CapitalizeFirst() : eventDef.LabelCap;

		public override void Notify_HistoryEvent(HistoryEvent ev, Precept precept)
		{
			if (ev.def != eventDef || precept.ideo != Faction.OfPlayer.ideos.FluidIdeo || !precept.ideo.development.CanBeDevelopedNow)
			{
				return;
			}
			ev.args.TryGetArg(HistoryEventArgsNames.Ideo, out Ideo arg);
			if (arg == null || arg == precept.ideo)
			{
				int num = precept.ideo.development.Points;
				if (precept.ideo.development.TryAddDevelopmentPoints(points))
				{
					ev.args.TryGetArg(HistoryEventArgsNames.Doer, out Pawn arg2);
					ev.args.TryGetArg(HistoryEventArgsNames.Quest, out Quest arg3);
					Messages.Message("MessageDevelopmentPointsEarned".Translate(num, precept.ideo.development.Points, Label), arg2, MessageTypeDefOf.PositiveEvent, arg3);
				}
			}
		}

		public override IEnumerable<string> GetDescriptions()
		{
			yield return LabelCap + ": " + points.ToStringWithSign();
		}
	}
}
