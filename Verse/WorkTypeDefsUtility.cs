using System.Collections.Generic;
using System.Linq;

namespace Verse
{
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
			default:
				Log.Error("Unknown or mixed worktags for naming: " + (int)tags);
				return "Worktag";
			}
		}

		public static bool OverlapsWithOnAnyWorkType(this WorkTags a, WorkTags b)
		{
			List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				WorkTypeDef workTypeDef = allDefsListForReading[i];
				if ((workTypeDef.workTags & a) != 0 && (workTypeDef.workTags & b) != 0)
				{
					return true;
				}
			}
			return false;
		}
	}
}
