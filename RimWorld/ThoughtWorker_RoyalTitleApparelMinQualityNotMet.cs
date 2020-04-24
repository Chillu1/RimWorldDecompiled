using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_RoyalTitleApparelMinQualityNotMet : ThoughtWorker
	{
		private RoyalTitleDef Validate(Pawn p, out QualityCategory minQuality)
		{
			minQuality = QualityCategory.Awful;
			foreach (RoyalTitle item in p.royalty.AllTitlesInEffectForReading)
			{
				List<Apparel> wornApparel = p.apparel.WornApparel;
				minQuality = item.def.requiredMinimumApparelQuality;
				for (int i = 0; i < wornApparel.Count; i++)
				{
					if (wornApparel[i].TryGetQuality(out QualityCategory qc) && (int)qc < (int)item.def.requiredMinimumApparelQuality)
					{
						return item.def;
					}
				}
			}
			return null;
		}

		public override string PostProcessLabel(Pawn p, string label)
		{
			QualityCategory minQuality;
			RoyalTitleDef royalTitleDef = Validate(p, out minQuality);
			if (royalTitleDef == null)
			{
				return string.Empty;
			}
			return label.Formatted(royalTitleDef.GetLabelCapFor(p).Named("TITLE"), p.Named("PAWN"));
		}

		public override string PostProcessDescription(Pawn p, string description)
		{
			QualityCategory minQuality;
			RoyalTitleDef royalTitleDef = Validate(p, out minQuality);
			if (royalTitleDef == null)
			{
				return string.Empty;
			}
			return description.Formatted(royalTitleDef.GetLabelCapFor(p).Named("TITLE"), minQuality.GetLabel().Named("QUALITY"), p.Named("PAWN"));
		}

		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.royalty == null)
			{
				return false;
			}
			if (Validate(p, out QualityCategory _) == null)
			{
				return ThoughtState.Inactive;
			}
			return ThoughtState.ActiveAtStage(0);
		}
	}
}
