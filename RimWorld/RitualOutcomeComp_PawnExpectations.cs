using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualOutcomeComp_PawnExpectations : RitualOutcomeComp_QualitySingleOffset
{
	[NoTranslate]
	public string roleId;

	public float offsetForSlave;

	public List<PawnExpectationsQualityOffset> offsetPerExpectation = new List<PawnExpectationsQualityOffset>();

	private PawnExpectationsQualityOffset slaveExpectationCached;

	public PawnExpectationsQualityOffset SlaveExpectationCached
	{
		get
		{
			if (slaveExpectationCached == null)
			{
				slaveExpectationCached = new PawnExpectationsQualityOffset
				{
					labelOverride = "Slave".Translate().CapitalizeFirst(),
					offset = offsetForSlave
				};
			}
			return slaveExpectationCached;
		}
	}

	public override float QualityOffset(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
	{
		return Count(ritual, data);
	}

	protected PawnExpectationsQualityOffset ExpectationOffset(Pawn pawn)
	{
		if (pawn.IsSlaveOfColony)
		{
			return SlaveExpectationCached;
		}
		ExpectationDef expectations = ExpectationsUtility.CurrentExpectationFor(pawn);
		return offsetPerExpectation.FirstOrDefault((PawnExpectationsQualityOffset e) => e.expectation == expectations);
	}

	public override float Count(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
	{
		Pawn pawn = ritual.assignments.FirstAssignedPawn(roleId);
		if (pawn == null)
		{
			return 0f;
		}
		return ExpectationOffset(pawn)?.offset ?? 0f;
	}

	public override string GetDesc(LordJob_Ritual ritual = null, RitualOutcomeComp_Data data = null)
	{
		if (ritual == null)
		{
			return labelAbstract;
		}
		Pawn pawn = ritual?.PawnWithRole(roleId);
		if (pawn == null)
		{
			return null;
		}
		PawnExpectationsQualityOffset pawnExpectationsQualityOffset = ExpectationOffset(pawn);
		string text = ((pawnExpectationsQualityOffset.offset < 0f) ? "" : "+");
		return LabelForDesc.Formatted(pawn.Named("PAWN")) + ": " + "OutcomeBonusDesc_QualitySingleOffset".Translate(text + pawnExpectationsQualityOffset.offset.ToStringPercent()) + ".";
	}

	public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
	{
		Pawn pawn = assignments.FirstAssignedPawn(roleId);
		if (pawn == null)
		{
			return null;
		}
		PawnExpectationsQualityOffset pawnExpectationsQualityOffset = ExpectationOffset(pawn);
		if (pawnExpectationsQualityOffset != null)
		{
			return new QualityFactor
			{
				label = "RitualPredictedOutcomeDescPawnExpectations".Translate(pawn.Named("PAWN")),
				count = pawnExpectationsQualityOffset.Label,
				qualityChange = ((Math.Abs(pawnExpectationsQualityOffset.offset) > float.Epsilon) ? "OutcomeBonusDesc_QualitySingleOffset".Translate(pawnExpectationsQualityOffset.offset.ToStringWithSign("0.#%")).Resolve() : " - "),
				positive = (pawnExpectationsQualityOffset.offset >= 0f),
				quality = pawnExpectationsQualityOffset.offset,
				priority = 0f
			};
		}
		return null;
	}
}
