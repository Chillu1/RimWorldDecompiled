using System.Collections.Generic;
using System.Linq;

namespace Verse;

public static class WorkTypeDefsUtility
{
	public static IEnumerable<WorkTypeDef> WorkTypeDefsInPriorityOrder => DefDatabase<WorkTypeDef>.AllDefs.OrderByDescending((WorkTypeDef wt) => wt.naturalPriority);

	public static string LabelTranslated(this WorkTags tags)
	{
		switch (tags)
		{
		case WorkTags.None:
			return "WorkTagNone".Translate();
		case WorkTags.Intellectual:
			return "WorkTagIntellectual".Translate();
		case WorkTags.ManualDumb:
			return "WorkTagManualDumb".Translate();
		case WorkTags.ManualSkilled:
			return "WorkTagManualSkilled".Translate();
		case WorkTags.Violent:
			return "WorkTagViolent".Translate();
		case WorkTags.Caring:
			return "WorkTagCaring".Translate();
		case WorkTags.Social:
			return "WorkTagSocial".Translate();
		case WorkTags.Commoner:
			return "WorkTagCommoner".Translate();
		case WorkTags.Animals:
			return "WorkTagAnimals".Translate();
		case WorkTags.Artistic:
			return "WorkTagArtistic".Translate();
		case WorkTags.Crafting:
			return "WorkTagCrafting".Translate();
		case WorkTags.Cooking:
			return "WorkTagCooking".Translate();
		case WorkTags.Firefighting:
			return "WorkTagFirefighting".Translate();
		case WorkTags.Cleaning:
			return "WorkTagCleaning".Translate();
		case WorkTags.Hauling:
			return "WorkTagHauling".Translate();
		case WorkTags.PlantWork:
			return "WorkTagPlantWork".Translate();
		case WorkTags.Mining:
			return "WorkTagMining".Translate();
		case WorkTags.Hunting:
			return "WorkTagHunting".Translate();
		case WorkTags.Constructing:
			return "WorkTagConstructing".Translate();
		case WorkTags.Shooting:
			return "WorkTagShooting".Translate();
		case WorkTags.AllWork:
			return "WorkTagAllWork".Translate();
		default:
		{
			int num = (int)tags;
			Log.Error("Unknown or mixed worktags for naming: " + num);
			return "Worktag";
		}
		}
	}

	public static bool ExactlyOneWorkTagSet(this WorkTags workTags)
	{
		if (workTags != WorkTags.None)
		{
			return (workTags & (workTags - 1)) == 0;
		}
		return false;
	}

	public static bool OverlapsWithOnAnyWorkType(this WorkTags a, WorkTags b)
	{
		List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			WorkTypeDef workTypeDef = allDefsListForReading[i];
			if ((workTypeDef.workTags & a) != WorkTags.None && (workTypeDef.workTags & b) != WorkTags.None)
			{
				return true;
			}
		}
		return false;
	}
}
