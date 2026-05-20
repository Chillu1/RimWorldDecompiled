using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class RitualAttachableOutcomeEffectWorker_DiscoverAncientComplex : RitualAttachableOutcomeEffectWorker
{
	public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
	{
		if (!CanApplyNow(jobRitual.Ritual, jobRitual.Map))
		{
			extraOutcomeDesc = null;
			return;
		}
		extraOutcomeDesc = def.letterInfoText;
		Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDefOf.OpportunitySite_AncientComplex, StorytellerUtility.DefaultThreatPointsNow(jobRitual.Map));
		letterLookTargets = new LookTargets((letterLookTargets.targets ?? new List<GlobalTargetInfo>()).Concat(quest.QuestLookTargets));
	}

	public override AcceptanceReport CanApplyNow(Precept_Ritual ritual, Map map)
	{
		foreach (WorldObject allWorldObject in Find.WorldObjects.AllWorldObjects)
		{
			if (allWorldObject is Site site && site.parts.Any((SitePart p) => p.def == SitePartDefOf.AncientComplex))
			{
				return "RitualAttachedOutcomeCantApply_AncientComplexIsPresent".Translate();
			}
		}
		foreach (Quest item in Find.QuestManager.QuestsListForReading)
		{
			if (item.root == QuestScriptDefOf.AncientComplex_Mission && !item.Historical)
			{
				return "RitualAttachedOutcomeCantApply_AncientComplexIsPresent".Translate();
			}
		}
		float points = StorytellerUtility.DefaultThreatPointsNow(map);
		if (!QuestScriptDefOf.OpportunitySite_AncientComplex.CanRun(points, map))
		{
			return "RitualAttachedOutcomeCantApply_NotAvailableRightNow".Translate();
		}
		return base.CanApplyNow(ritual, map);
	}
}
