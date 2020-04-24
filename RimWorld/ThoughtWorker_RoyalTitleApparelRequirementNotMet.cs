using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_RoyalTitleApparelRequirementNotMet : ThoughtWorker
	{
		private static RoyalTitleDef Validate(Pawn p)
		{
			if (p.royalty == null || !p.royalty.allowApparelRequirements)
			{
				return null;
			}
			foreach (RoyalTitle item in p.royalty.AllTitlesInEffectForReading)
			{
				if (item.def.requiredApparel != null && item.def.requiredApparel.Count > 0)
				{
					for (int i = 0; i < item.def.requiredApparel.Count; i++)
					{
						if (!item.def.requiredApparel[i].IsMet(p))
						{
							return item.def;
						}
					}
				}
			}
			return null;
		}

		private static IEnumerable<string> GetFirstRequiredApparelPerGroup(Pawn p)
		{
			if (p.royalty != null && p.royalty.allowApparelRequirements)
			{
				foreach (RoyalTitle t in p.royalty.AllTitlesInEffectForReading)
				{
					if (t.def.requiredApparel != null && t.def.requiredApparel.Count > 0)
					{
						for (int i = 0; i < t.def.requiredApparel.Count; i++)
						{
							RoyalTitleDef.ApparelRequirement apparelRequirement = t.def.requiredApparel[i];
							if (!apparelRequirement.IsMet(p))
							{
								yield return apparelRequirement.AllRequiredApparelForPawn(p).First().LabelCap;
							}
						}
					}
				}
				yield return "ApparelRequirementAnyPowerArmor".Translate();
				yield return "ApparelRequirementAnyPsycasterApparel".Translate();
			}
		}

		public override string PostProcessLabel(Pawn p, string label)
		{
			RoyalTitleDef royalTitleDef = Validate(p);
			if (royalTitleDef == null)
			{
				return string.Empty;
			}
			return label.Formatted(royalTitleDef.GetLabelCapFor(p).Named("TITLE"), p.Named("PAWN"));
		}

		public override string PostProcessDescription(Pawn p, string description)
		{
			RoyalTitleDef royalTitleDef = Validate(p);
			if (royalTitleDef == null)
			{
				return string.Empty;
			}
			return description.Formatted(GetFirstRequiredApparelPerGroup(p).ToLineList("- "), royalTitleDef.GetLabelCapFor(p).Named("TITLE"), p.Named("PAWN"));
		}

		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (Validate(p) == null)
			{
				return ThoughtState.Inactive;
			}
			return ThoughtState.ActiveDefault;
		}
	}
}
