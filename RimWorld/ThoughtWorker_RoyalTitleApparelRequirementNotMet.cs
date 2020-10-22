using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_RoyalTitleApparelRequirementNotMet : ThoughtWorker
	{
		[Obsolete("Will be removed in the future")]
		private static RoyalTitleDef Validate(Pawn p)
		{
			return null;
		}

		private static RoyalTitle Validate_NewTemp(Pawn p)
		{
			if (p.royalty == null || !p.royalty.allowApparelRequirements)
			{
				return null;
			}
			foreach (RoyalTitle item in p.royalty.AllTitlesInEffectForReading)
			{
				if (item.def.requiredApparel == null || item.def.requiredApparel.Count <= 0)
				{
					continue;
				}
				for (int i = 0; i < item.def.requiredApparel.Count; i++)
				{
					if (!item.def.requiredApparel[i].IsMet(p))
					{
						return item;
					}
				}
			}
			return null;
		}

		[Obsolete("Only used for mod compatibility. Will be removed in a future update.")]
		private static IEnumerable<string> GetFirstRequiredApparelPerGroup(Pawn p)
		{
			if (p.royalty == null || !p.royalty.allowApparelRequirements)
			{
				yield break;
			}
			foreach (RoyalTitle t in p.royalty.AllTitlesInEffectForReading)
			{
				if (t.def.requiredApparel == null || t.def.requiredApparel.Count <= 0)
				{
					continue;
				}
				for (int i = 0; i < t.def.requiredApparel.Count; i++)
				{
					RoyalTitleDef.ApparelRequirement apparelRequirement = t.def.requiredApparel[i];
					if (!apparelRequirement.IsMet(p))
					{
						yield return apparelRequirement.AllRequiredApparelForPawn(p).First().LabelCap;
					}
				}
			}
			yield return "ApparelRequirementAnyPrestigeArmor".Translate();
			yield return "ApparelRequirementAnyPsycasterApparel".Translate();
		}

		private static IEnumerable<string> GetAllRequiredApparelPerGroup(Pawn p)
		{
			if (p.royalty == null || !p.royalty.allowApparelRequirements)
			{
				yield break;
			}
			foreach (RoyalTitle t in p.royalty.AllTitlesInEffectForReading)
			{
				if (t.def.requiredApparel == null || t.def.requiredApparel.Count <= 0)
				{
					continue;
				}
				for (int i = 0; i < t.def.requiredApparel.Count; i++)
				{
					RoyalTitleDef.ApparelRequirement apparelRequirement = t.def.requiredApparel[i];
					if (apparelRequirement.IsMet(p))
					{
						continue;
					}
					IEnumerable<ThingDef> enumerable = apparelRequirement.AllRequiredApparelForPawn(p);
					foreach (ThingDef item in enumerable)
					{
						yield return item.LabelCap;
					}
				}
			}
			yield return "ApparelRequirementAnyPrestigeArmor".Translate();
			yield return "ApparelRequirementAnyPsycasterApparel".Translate();
		}

		public override string PostProcessLabel(Pawn p, string label)
		{
			RoyalTitle royalTitle = Validate_NewTemp(p);
			if (royalTitle == null)
			{
				return string.Empty;
			}
			return label.Formatted(royalTitle.Named("TITLE"), p.Named("PAWN")).CapitalizeFirst();
		}

		public override string PostProcessDescription(Pawn p, string description)
		{
			RoyalTitle royalTitle = Validate_NewTemp(p);
			if (royalTitle == null)
			{
				return string.Empty;
			}
			return description.Formatted(GetAllRequiredApparelPerGroup(p).Distinct().ToLineList("- "), royalTitle.Named("TITLE"), p.Named("PAWN")).CapitalizeFirst();
		}

		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (Validate_NewTemp(p) == null)
			{
				return ThoughtState.Inactive;
			}
			return ThoughtState.ActiveDefault;
		}
	}
}
