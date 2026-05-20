using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class PreceptWorker_Apparel : PreceptWorker
{
	public override IEnumerable<PreceptThingChance> ThingDefs
	{
		get
		{
			foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => IsValidApparel(x)))
			{
				yield return new PreceptThingChance
				{
					def = item,
					chance = 1f
				};
			}
		}
	}

	public override bool ShouldSkipThing(Ideo ideo, ThingDef thingDef)
	{
		foreach (Precept item in ideo.PreceptsListForReading)
		{
			if (item is Precept_Apparel precept_Apparel && precept_Apparel.apparelDef == thingDef)
			{
				return true;
			}
		}
		return false;
	}

	public override float GetThingOrder(PreceptThingChance thingChance)
	{
		if (thingChance.def.IsApparel)
		{
			return (thingChance.def.apparel.LastLayer == ApparelLayerDefOf.Overhead || thingChance.def.apparel.LastLayer == ApparelLayerDefOf.EyeCover) ? 1 : 2;
		}
		return 0f;
	}

	private bool IsValidApparel(ThingDef td)
	{
		if (!td.IsApparel || !td.MadeFromStuff)
		{
			return false;
		}
		if (!td.apparel.canBeDesiredForIdeo)
		{
			return false;
		}
		if (td.thingCategories != null && (td.thingCategories.Contains(ThingCategoryDefOf.ArmorHeadgear) || td.thingCategories.Contains(ThingCategoryDefOf.ApparelArmor)))
		{
			return false;
		}
		for (int i = 0; i < def.comps.Count; i++)
		{
			if (def.comps[i] is PreceptComp_Apparel preceptComp_Apparel && !preceptComp_Apparel.CanApplyToApparel(td))
			{
				return false;
			}
		}
		return true;
	}

	public override AcceptanceReport CanUse(ThingDef def, Ideo ideo, FactionDef generatingFor)
	{
		if (IsValidApparel(def))
		{
			if (generatingFor != null)
			{
				AcceptanceReport result = CheckFaction(generatingFor);
				if (!result.Accepted)
				{
					return result;
				}
			}
			if (Find.World != null && Find.FactionManager != null)
			{
				foreach (Faction allFaction in Find.FactionManager.AllFactions)
				{
					if (allFaction.def != generatingFor && allFaction.ideos != null && allFaction.ideos.PrimaryIdeo == ideo)
					{
						AcceptanceReport acceptanceReport = CheckFaction(allFaction.def);
						if (!acceptanceReport)
						{
							return acceptanceReport;
						}
					}
				}
			}
			return true;
		}
		return false;
		AcceptanceReport CheckFaction(FactionDef faction)
		{
			if (def.apparel.ideoDesireAllowedFactionCategoryTags != null && !def.apparel.ideoDesireAllowedFactionCategoryTags.Contains(faction.categoryTag))
			{
				return new AcceptanceReport("RoleApparelRequirementIncompatibleFaction".Translate(Find.ActiveLanguageWorker.WithIndefiniteArticle(def.apparel.ideoDesireAllowedFactionCategoryTags.Select((string t) => ("RoleApparelRequirementIncompatibleFaction_Allowed_" + t).Translate().Resolve()).ToCommaListOr())));
			}
			if (def.apparel.ideoDesireDisallowedFactionCategoryTags != null && def.apparel.ideoDesireDisallowedFactionCategoryTags.Contains(faction.categoryTag))
			{
				return new AcceptanceReport("RoleApparelRequirementIncompatibleFaction".Translate(Find.ActiveLanguageWorker.WithIndefiniteArticle(def.apparel.ideoDesireDisallowedFactionCategoryTags.Select((string t) => ("RoleApparelRequirementIncompatibleFaction_Disallowed_" + t).Translate().Resolve()).ToCommaListOr())));
			}
			return true;
		}
	}
}
