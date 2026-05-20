using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ThoughtWorker_RoyalTitleApparelMinQualityNotMet : ThoughtWorker
{
	private RoyalTitle Validate(Pawn p, out QualityCategory minQuality)
	{
		minQuality = QualityCategory.Awful;
		foreach (RoyalTitle item in p.royalty.AllTitlesInEffectForReading)
		{
			List<Apparel> wornApparel = p.apparel.WornApparel;
			minQuality = item.def.requiredMinimumApparelQuality;
			for (int i = 0; i < wornApparel.Count; i++)
			{
				if (wornApparel[i].TryGetQuality(out var qc) && (int)qc < (int)item.def.requiredMinimumApparelQuality)
				{
					return item;
				}
			}
		}
		return null;
	}

	public override string PostProcessLabel(Pawn p, string label)
	{
		QualityCategory minQuality;
		RoyalTitle royalTitle = Validate(p, out minQuality);
		if (royalTitle == null)
		{
			return string.Empty;
		}
		return label.Formatted(royalTitle.Named("TITLE"), p.Named("PAWN"));
	}

	public override string PostProcessDescription(Pawn p, string description)
	{
		QualityCategory minQuality;
		RoyalTitle royalTitle = Validate(p, out minQuality);
		if (royalTitle == null)
		{
			return string.Empty;
		}
		return description.Formatted(royalTitle.Named("TITLE"), minQuality.GetLabel().Named("QUALITY"), p.Named("PAWN")).CapitalizeFirst();
	}

	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (p.royalty == null)
		{
			return false;
		}
		if (Validate(p, out var _) == null)
		{
			return ThoughtState.Inactive;
		}
		return ThoughtState.ActiveAtStage(0);
	}
}
