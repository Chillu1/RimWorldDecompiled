using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_ShuttleLeaveDelay : QuestPart_Delay
	{
		public Thing shuttle;

		public List<string> inSignalsDisable = new List<string>();

		public override AlertReport AlertReport
		{
			get
			{
				if (shuttle == null)
				{
					return false;
				}
				return AlertReport.CulpritIs(shuttle);
			}
		}

		public override bool AlertCritical => base.TicksLeft < 60000;

		public override string AlertLabel => "QuestPartShuttleLeaveDelay".Translate(base.TicksLeft.ToStringTicksToPeriod());

		public override string AlertExplanation => "QuestPartShuttleLeaveDelayDesc".Translate(quest.name, base.TicksLeft.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor), shuttle.TryGetComp<CompShuttle>().RequiredThingsLabel);

		public override IEnumerable<GlobalTargetInfo> QuestLookTargets
		{
			get
			{
				foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
				{
					yield return questLookTarget;
				}
				if (shuttle != null)
				{
					yield return shuttle;
				}
			}
		}

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (inSignalsDisable.Contains(signal.tag))
			{
				Disable();
			}
		}

		public override string ExtraInspectString(ISelectable target)
		{
			if (target == shuttle)
			{
				return "ShuttleLeaveDelayInspectString".Translate(base.TicksLeft.ToStringTicksToPeriod());
			}
			return null;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref shuttle, "shuttle");
			Scribe_Collections.Look(ref inSignalsDisable, "inSignalsDisable", LookMode.Value);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && inSignalsDisable == null)
			{
				inSignalsDisable = new List<string>();
			}
		}

		public override void ReplacePawnReferences(Pawn replace, Pawn with)
		{
			if (shuttle != null)
			{
				shuttle.TryGetComp<CompShuttle>().requiredPawns.Replace(replace, with);
			}
		}
	}
}
