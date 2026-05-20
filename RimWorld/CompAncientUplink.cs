using System.Collections.Generic;
using System.Linq;
using RimWorld.QuestGen;
using Verse;

namespace RimWorld;

public class CompAncientUplink : ThingComp
{
	private List<QuestScriptDef> scannerQuests;

	private List<QuestScriptDef> ScannerQuests => scannerQuests ?? (scannerQuests = QuestUtility.GetGiverQuests(QuestGiverTag.OrbitalScanner).ToList());

	public override void Notify_Hacked(Pawn hacker = null)
	{
		if (ModsConfig.OdysseyActive)
		{
			float points = StorytellerUtility.DefaultThreatPointsNow(parent.Map);
			Slate slate = new Slate();
			slate.Set("points", points);
			slate.Set("discoveryMethod", "QuestDiscoveredFromUplink".Translate(hacker.Named("HACKER")));
			QuestScriptDef questScriptDef = ScannerQuests.RandomElementByWeight((QuestScriptDef q) => NaturalRandomQuestChooser.GetNaturalRandomSelectionWeight(q, points, Find.World.StoryState));
			Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(questScriptDef, slate);
			if (!quest.hidden && questScriptDef.sendAvailableLetter)
			{
				QuestUtility.SendLetterQuestAvailable(quest, slate.Get<string>("discoveryMethod"));
			}
		}
	}
}
