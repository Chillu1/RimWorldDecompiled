using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

public static class MechWorkUtility
{
	public static IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef, StatRequest req)
	{
		if (!parentDef.race.IsMechanoid || parentDef.race.mechEnabledWorkTypes.NullOrEmpty())
		{
			yield break;
		}
		TaggedString taggedString = "MechWorkActivitiesExplanation".Translate() + ":\n";
		foreach (WorkTypeDef item in parentDef.race.mechEnabledWorkTypes.OrderBy((WorkTypeDef wt) => wt.label))
		{
			IEnumerable<WorkGiverDef> source = item.workGiversByPriority.Where((WorkGiverDef wg) => wg.canBeDoneByMechs);
			if (!source.Any())
			{
				continue;
			}
			taggedString += "\n - " + item.gerundLabel.CapitalizeFirst();
			foreach (WorkGiverDef item2 in source.OrderBy((WorkGiverDef wg) => wg.label))
			{
				taggedString += "\n  - " + item2.LabelCap;
			}
		}
		yield return new StatDrawEntry(StatCategoryDefOf.PawnWork, "MechWorkActivities".Translate(), parentDef.race.mechEnabledWorkTypes.Select((WorkTypeDef w) => w.gerundLabel).ToCommaList(useAnd: true).CapitalizeFirst(), taggedString, 502);
		yield return new StatDrawEntry(StatCategoryDefOf.PawnWork, "MechWorkSkill".Translate(), parentDef.race.mechFixedSkillLevel.ToString(), "MechWorkSkillDesc".Translate(), 501);
	}

	public static bool AnyWorkMechCouldDo(RecipeDef recipe)
	{
		IEnumerable<WorkTypeDef> mechEnabledWorkTypes = DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef p) => p.RaceProps.IsWorkMech).SelectMany((PawnKindDef p) => p.RaceProps.mechEnabledWorkTypes).Distinct();
		if (recipe.requiredGiverWorkType != null && !mechEnabledWorkTypes.Contains(recipe.requiredGiverWorkType))
		{
			return false;
		}
		IEnumerable<ThingDef> recipeUsers = recipe.AllRecipeUsers;
		return DefDatabase<WorkGiverDef>.AllDefs.Where((WorkGiverDef wg) => !wg.fixedBillGiverDefs.NullOrEmpty() && wg.fixedBillGiverDefs.Intersect(recipeUsers).Any()).Any((WorkGiverDef wg) => mechEnabledWorkTypes.Contains(wg.workType));
	}
}
