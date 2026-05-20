using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_ShuttleDelay : QuestPart_Delay
{
	public List<Pawn> lodgers = new List<Pawn>();

	public bool alert;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			for (int i = 0; i < lodgers.Count; i++)
			{
				yield return lodgers[i];
			}
		}
	}

	public override AlertReport AlertReport
	{
		get
		{
			if (!alert || base.State != QuestPartState.Enabled)
			{
				return false;
			}
			return AlertReport.CulpritsAre(lodgers);
		}
	}

	public override bool AlertCritical => base.TicksLeft < 60000;

	public override string AlertLabel => "QuestPartShuttleArriveDelay".Translate(base.TicksLeft.ToStringTicksToPeriodVerbose());

	public override string AlertExplanation
	{
		get
		{
			if (quest.hidden)
			{
				return "QuestPartShuttleArriveDelayDescHidden".Translate(base.TicksLeft.ToStringTicksToPeriodVerbose().Colorize(ColoredText.DateTimeColor));
			}
			return "QuestPartShuttleArriveDelayDesc".Translate(quest.name, base.TicksLeft.ToStringTicksToPeriodVerbose().Colorize(ColoredText.DateTimeColor), lodgers.Select((Pawn p) => p.LabelShort).ToLineList("- "));
		}
	}

	public override string ExtraInspectString(ISelectable target)
	{
		if (target is Pawn item && lodgers.Contains(item))
		{
			return "ShuttleDelayInspectString".Translate(base.TicksLeft.ToStringTicksToPeriod());
		}
		return null;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref lodgers, "lodgers", LookMode.Reference);
		Scribe_Values.Look(ref alert, "alert", defaultValue: false);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			lodgers.RemoveAll((Pawn x) => x == null);
		}
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		if (Find.AnyPlayerHomeMap != null)
		{
			lodgers.AddRange(Find.RandomPlayerHomeMap.mapPawns.FreeColonists);
		}
	}

	public override void ReplacePawnReferences(Pawn replace, Pawn with)
	{
		lodgers.Replace(replace, with);
	}
}
