using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Alert_AgeReversalDemandNear : Alert
{
	private const float WarnWhenCloserThanTicks = 300000f;

	private const float WarnWhenCloserThanTicksAutoAgeReverse = -60000f;

	private List<Pawn> targets = new List<Pawn>();

	public Alert_AgeReversalDemandNear()
	{
		defaultLabel = "AlertAgeReversalDemandNear".Translate();
		requireIdeology = true;
	}

	private void CalcPawnsNearDeadline()
	{
		targets.Clear();
		foreach (Pawn item in PawnsFinder.AllMaps_FreeColonistsSpawned)
		{
			Ideo ideo = item.Ideo;
			if (ideo != null && ideo.HasPrecept(PreceptDefOf.AgeReversal_Demanded) && ThoughtWorker_AgeReversalDemanded.CanHaveThought(item))
			{
				long ageReversalDemandedDeadlineTicks = item.ageTracker.AgeReversalDemandedDeadlineTicks;
				float num = (CompBiosculpterPod.HasBiotunedAutoAgeReversePod(item) ? (-60000f) : 300000f);
				if ((float)ageReversalDemandedDeadlineTicks <= num)
				{
					targets.Add(item);
				}
			}
		}
	}

	public override TaggedString GetExplanation()
	{
		TaggedString result = "AlertAgeReversalDemandDesc".Translate();
		foreach (Pawn target in targets)
		{
			long num = target.ageTracker.AgeReversalDemandedDeadlineTicks;
			string key;
			if (num > 0)
			{
				key = ((target.ageTracker.LastAgeReversalReason != Pawn_AgeTracker.AgeReversalReason.Recruited) ? ((target.ageTracker.LastAgeReversalReason != Pawn_AgeTracker.AgeReversalReason.ViaTreatment) ? "AlertAgeReversalDemandDesc_Initial" : "AlertAgeReversalDemandDesc_Next") : "AlertAgeReversalDemandDesc_Recruit");
			}
			else
			{
				num = -num;
				key = "AlertAgeReversalDemandDesc_Overdue";
			}
			result += "\n  - " + key.Translate(target.Named("PAWN"), target.Faction.Named("FACTION"), ((int)num).ToStringTicksToPeriodVerbose().Named("DURATION"));
		}
		return result;
	}

	public override AlertReport GetReport()
	{
		CalcPawnsNearDeadline();
		return AlertReport.CulpritsAre(targets);
	}
}
