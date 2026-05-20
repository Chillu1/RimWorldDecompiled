using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class ThoughtWorker_RoyalTitleApparelRequirementNotMet : ThoughtWorker
{
	private static RoyalTitle Validate(Pawn p)
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
				ApparelRequirement apparelRequirement = item.def.requiredApparel[i];
				if (apparelRequirement.IsActive(p) && !apparelRequirement.IsMet(p))
				{
					return item;
				}
			}
		}
		return null;
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
			for (int j = 0; j < t.def.requiredApparel.Count; j++)
			{
				ApparelRequirement apparelRequirement = t.def.requiredApparel[j];
				if (!apparelRequirement.IsActive(p) || apparelRequirement.IsMet(p))
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
		if (ModsConfig.BiotechActive)
		{
			yield return "ApparelRequirementAnyMechlordApparel".Translate();
		}
	}

	public override string PostProcessLabel(Pawn p, string label)
	{
		RoyalTitle royalTitle = Validate(p);
		if (royalTitle == null)
		{
			return string.Empty;
		}
		return label.Formatted(royalTitle.Named("TITLE"), p.Named("PAWN")).CapitalizeFirst();
	}

	public override string PostProcessDescription(Pawn p, string description)
	{
		RoyalTitle royalTitle = Validate(p);
		if (royalTitle == null)
		{
			return string.Empty;
		}
		return description.Formatted(GetAllRequiredApparelPerGroup(p).Distinct().ToLineList("- "), royalTitle.Named("TITLE"), p.Named("PAWN")).CapitalizeFirst();
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
