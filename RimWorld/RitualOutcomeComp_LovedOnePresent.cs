using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class RitualOutcomeComp_LovedOnePresent : RitualOutcomeComp_Quality
{
	[MustTranslate]
	public string labelNotMet;

	[NoTranslate]
	public string lovedOneOfRole;

	private static List<Pawn> lovedOnesTmp = new List<Pawn>();

	public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
	{
		Pawn pawn = LovedOneParticipating(assignments);
		bool flag = pawn != null;
		float quality = (flag ? qualityOffset : 0f);
		return new QualityFactor
		{
			label = "RitualOutcomeLovedOnePresentLabel".Translate(),
			present = flag,
			qualityChange = ExpectedOffsetDesc(positive: true, quality),
			quality = quality,
			positive = true,
			priority = 4f,
			count = pawn?.LabelShort,
			toolTip = "RitualOutcomeLovedOnePresentTip".Translate(assignments.FirstAssignedPawn(lovedOneOfRole).Named("PAWN"))
		};
	}

	private Pawn LovedOneParticipating(RitualRoleAssignments assignments)
	{
		Pawn lovedOnOf = assignments.FirstAssignedPawn(lovedOneOfRole);
		if (lovedOnOf == null)
		{
			return null;
		}
		lovedOnesTmp.Clear();
		lovedOnesTmp.AddRange(from p in SocialCardUtility.PawnsForSocialInfo(lovedOnOf)
			where assignments.PawnParticipating(p) && lovedOnOf.relations.OpinionOf(p) >= 20
			select p);
		if (lovedOnesTmp.Count > 0)
		{
			lovedOnesTmp.SortByDescending((Pawn l) => lovedOnOf.relations.OpinionOf(l));
			return lovedOnesTmp.First();
		}
		return null;
	}

	public override string GetDesc(LordJob_Ritual ritual = null, RitualOutcomeComp_Data data = null)
	{
		string text = ((qualityOffset < 0f) ? "" : "+");
		return ((LovedOneParticipating(ritual.assignments) != null) ? label : labelNotMet).CapitalizeFirst().Formatted() + ": " + "OutcomeBonusDesc_QualitySingleOffset".Translate(text + qualityOffset.ToStringPercent()) + ".";
	}

	public override float QualityOffset(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
	{
		if (LovedOneParticipating(ritual.assignments) == null)
		{
			return 0f;
		}
		return qualityOffset;
	}

	protected override string ExpectedOffsetDesc(bool positive, float quality = 0f)
	{
		if (!positive)
		{
			return "";
		}
		return quality.ToStringWithSign("0.#%");
	}

	public override float Count(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
	{
		return (LovedOneParticipating(ritual.assignments) != null) ? 1 : 0;
	}
}
