using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IngestionOutcomeDoer_OffsetNeed : IngestionOutcomeDoer
	{
		public NeedDef need;

		public float offset;

		public ChemicalDef toleranceChemical;

		protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
		{
			if (pawn.needs != null)
			{
				Need need = pawn.needs.TryGetNeed(this.need);
				if (need != null)
				{
					float effect = offset;
					AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, toleranceChemical, ref effect);
					need.CurLevel += effect;
				}
			}
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, need.LabelCap, offset.ToStringPercent(), need.description, need.listPriority);
		}
	}
}
